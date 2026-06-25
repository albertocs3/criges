namespace CriGes.Modules.Platform.Application.Customers;

public sealed record CustomerSummary(
    Guid Id,
    string Name,
    string? TaxId,
    string? Email,
    string? Phone,
    byte Status,
    DateTime CreatedAtUtc);
