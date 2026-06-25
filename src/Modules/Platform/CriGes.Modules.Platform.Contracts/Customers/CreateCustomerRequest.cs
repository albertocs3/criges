namespace CriGes.Modules.Platform.Contracts.Customers;

public sealed record CreateCustomerRequest(
    string? Name,
    string? TaxId,
    string? Email,
    string? Phone);
