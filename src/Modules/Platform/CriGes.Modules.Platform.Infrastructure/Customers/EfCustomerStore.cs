using System.Text.Json;
using CriGes.Modules.Platform.Application.Customers;
using CriGes.Modules.Platform.Infrastructure.Persistence;
using CriGes.Modules.Platform.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace CriGes.Modules.Platform.Infrastructure.Customers;

public sealed class EfCustomerStore(PlatformDbContext dbContext) : ICustomerStore
{
    public async Task<IReadOnlyList<CustomerSummary>> ListCustomersAsync(CancellationToken cancellationToken)
    {
        return await dbContext.Customers
            .AsNoTracking()
            .OrderBy(customer => customer.Name)
            .Select(customer => new CustomerSummary(
                customer.CustomerId,
                customer.Name,
                customer.TaxId,
                customer.Email,
                customer.Phone,
                customer.Status,
                customer.CreatedAtUtc))
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    public Task<bool> IsTaxIdReservedAsync(string normalizedTaxId, CancellationToken cancellationToken)
    {
        return dbContext.Customers.AnyAsync(
            customer => customer.NormalizedTaxId == normalizedTaxId,
            cancellationToken);
    }

    public async Task<CustomerSummary> CreateCustomerAsync(
        CustomerCreationData customer,
        Guid? actorUserId,
        Guid correlationId,
        CancellationToken cancellationToken)
    {
        await using var transaction = await dbContext.Database
            .BeginTransactionAsync(cancellationToken)
            .ConfigureAwait(false);

        var now = customer.CreatedAtUtc.UtcDateTime;
        dbContext.Customers.Add(new CustomerEntity
        {
            CustomerId = customer.CustomerId,
            Name = customer.Name,
            NormalizedName = customer.NormalizedName,
            TaxId = customer.TaxId,
            NormalizedTaxId = customer.NormalizedTaxId,
            Email = customer.Email,
            Phone = customer.Phone,
            Status = 1,
            CreatedAtUtc = now,
            ModifiedAtUtc = now,
            CreatedByUserId = actorUserId,
            ModifiedByUserId = actorUserId
        });

        var actorDisplayName = actorUserId is null
            ? null
            : await dbContext.Users
                .AsNoTracking()
                .Where(user => user.UserId == actorUserId)
                .Select(user => user.FullName)
                .SingleOrDefaultAsync(cancellationToken)
                .ConfigureAwait(false);

        dbContext.AuditEvents.Add(new AuditEventEntity
        {
            OccurredAtUtc = now,
            ActorType = actorUserId is null ? (byte)2 : (byte)1,
            ActorUserId = actorUserId,
            ActorDisplayName = actorDisplayName,
            Module = "Customers",
            Action = "CustomerCreated",
            EntityType = "Customer",
            EntityId = customer.CustomerId.ToString("D"),
            Result = 1,
            NewValuesJson = JsonSerializer.Serialize(new
            {
                customer.CustomerId,
                customer.Name,
                customer.TaxId,
                customer.Email,
                customer.Phone,
                Status = "active"
            }),
            Description = $"Customer '{customer.Name}' created.",
            CorrelationId = correlationId,
            CreatedByNode = "CriGes.Api"
        });

        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);

        return new CustomerSummary(
            customer.CustomerId,
            customer.Name,
            customer.TaxId,
            customer.Email,
            customer.Phone,
            Status: 1,
            now);
    }
}
