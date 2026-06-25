namespace CriGes.Modules.Platform.Application.Customers;

public sealed record CustomerCreationData(
    Guid CustomerId,
    string Name,
    string NormalizedName,
    string? TaxId,
    string? NormalizedTaxId,
    string? Email,
    string? Phone,
    DateTimeOffset CreatedAtUtc);
