namespace CriGes.Modules.Platform.Application.Initialization;

public sealed record InitializePlatformCommand(
    CompanyInput Company,
    AdministratorInput Administrator);

public sealed record CompanyInput(
    string? LegalName,
    string? TradeName,
    string? TaxId,
    AddressInput Address,
    string? Phone,
    string? Email);

public sealed record AddressInput(
    string? Line,
    string? PostalCode,
    string? City,
    string? Region,
    string? CountryCode);

public sealed record AdministratorInput(
    string? FullName,
    string? UserName,
    string? Password);
