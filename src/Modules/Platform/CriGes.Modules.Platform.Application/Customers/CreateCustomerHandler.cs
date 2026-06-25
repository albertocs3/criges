using CriGes.Application.Abstractions;
using CriGes.SharedKernel;

namespace CriGes.Modules.Platform.Application.Customers;

public sealed class CreateCustomerHandler(
    ICustomerStore store,
    IIdGenerator idGenerator,
    IClock clock,
    ICorrelationContext correlationContext)
{
    public async Task<Result<CustomerSummary>> HandleAsync(
        CreateCustomerCommand command,
        Guid? actorUserId,
        CancellationToken cancellationToken)
    {
        var name = command.Name?.Trim();
        if (string.IsNullOrWhiteSpace(name) || name.Length is < 3 or > 200)
        {
            return Result.Failure<CustomerSummary>(CustomerErrors.ValidationFailed);
        }

        var taxId = string.IsNullOrWhiteSpace(command.TaxId) ? null : command.TaxId.Trim();
        var normalizedTaxId = taxId?.ToUpperInvariant();
        if (normalizedTaxId is { Length: > 0 } &&
            await store.IsTaxIdReservedAsync(normalizedTaxId, cancellationToken).ConfigureAwait(false))
        {
            return Result.Failure<CustomerSummary>(CustomerErrors.TaxIdAlreadyExists);
        }

        var customer = new CustomerCreationData(
            idGenerator.NewId(),
            name,
            name.ToUpperInvariant(),
            taxId,
            normalizedTaxId,
            string.IsNullOrWhiteSpace(command.Email) ? null : command.Email.Trim(),
            string.IsNullOrWhiteSpace(command.Phone) ? null : command.Phone.Trim(),
            clock.UtcNow);

        var created = await store.CreateCustomerAsync(
                customer,
                actorUserId,
                correlationContext.CorrelationId,
                cancellationToken)
            .ConfigureAwait(false);

        return Result.Success(created);
    }
}
