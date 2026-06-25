namespace CriGes.Modules.Platform.Application.Customers;

public interface ICustomerStore
{
    Task<IReadOnlyList<CustomerSummary>> ListCustomersAsync(CancellationToken cancellationToken);

    Task<bool> IsTaxIdReservedAsync(string normalizedTaxId, CancellationToken cancellationToken);

    Task<CustomerSummary> CreateCustomerAsync(
        CustomerCreationData customer,
        Guid? actorUserId,
        Guid correlationId,
        CancellationToken cancellationToken);
}
