using CriGes.Modules.Platform.Application.Customers;

namespace CriGes.Modules.Platform.Infrastructure.Customers;

public sealed class InMemoryCustomerStore : ICustomerStore
{
    private readonly object syncRoot = new();
    private readonly List<CustomerSummary> customers = [];

    public Task<IReadOnlyList<CustomerSummary>> ListCustomersAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        lock (syncRoot)
        {
            return Task.FromResult<IReadOnlyList<CustomerSummary>>(customers
                .OrderBy(customer => customer.Name)
                .ToArray());
        }
    }

    public Task<bool> IsTaxIdReservedAsync(string normalizedTaxId, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        lock (syncRoot)
        {
            return Task.FromResult(customers.Any(customer =>
                string.Equals(customer.TaxId, normalizedTaxId, StringComparison.OrdinalIgnoreCase)));
        }
    }

    public Task<CustomerSummary> CreateCustomerAsync(
        CustomerCreationData customer,
        Guid? actorUserId,
        Guid correlationId,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var summary = new CustomerSummary(
            customer.CustomerId,
            customer.Name,
            customer.TaxId,
            customer.Email,
            customer.Phone,
            Status: 1,
            customer.CreatedAtUtc.UtcDateTime);

        lock (syncRoot)
        {
            customers.Add(summary);
        }

        return Task.FromResult(summary);
    }
}
