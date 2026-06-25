namespace CriGes.Modules.Platform.Application.Customers;

public sealed record CreateCustomerCommand(
    string? Name,
    string? TaxId,
    string? Email,
    string? Phone);
