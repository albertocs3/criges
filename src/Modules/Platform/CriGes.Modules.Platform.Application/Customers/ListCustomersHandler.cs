namespace CriGes.Modules.Platform.Application.Customers;

public sealed class ListCustomersHandler(ICustomerStore store)
{
    public Task<IReadOnlyList<CustomerSummary>> HandleAsync(CancellationToken cancellationToken)
    {
        return store.ListCustomersAsync(cancellationToken);
    }
}
