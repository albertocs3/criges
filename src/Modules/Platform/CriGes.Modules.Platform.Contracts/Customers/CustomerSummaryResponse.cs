namespace CriGes.Modules.Platform.Contracts.Customers;

public sealed record CustomerSummaryResponse(
    Guid Id,
    string Name,
    string? TaxId,
    string? Email,
    string? Phone,
    string Status,
    DateTimeOffset CreatedAtUtc);
