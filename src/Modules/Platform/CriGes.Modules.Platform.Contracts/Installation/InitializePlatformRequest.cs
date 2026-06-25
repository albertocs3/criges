namespace CriGes.Modules.Platform.Contracts.Installation;

public sealed record InitializePlatformRequest(
    CompanyRequest Company,
    AdministratorRequest Administrator);

public sealed record CompanyRequest(
    string? LegalName,
    string? TradeName,
    string? TaxId,
    AddressRequest Address,
    string? Phone,
    string? Email);

public sealed record AddressRequest(
    string? Line,
    string? PostalCode,
    string? City,
    string? Region,
    string? CountryCode);

public sealed record AdministratorRequest(
    string? FullName,
    string? UserName,
    string? Password);
