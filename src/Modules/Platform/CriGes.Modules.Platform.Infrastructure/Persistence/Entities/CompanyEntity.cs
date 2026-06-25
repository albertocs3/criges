namespace CriGes.Modules.Platform.Infrastructure.Persistence.Entities;

public sealed class CompanyEntity
{
    public Guid CompanyId { get; set; }

    public byte SingletonKey { get; set; } = 1;

    public string LegalName { get; set; } = string.Empty;

    public string? TradeName { get; set; }

    public string TaxId { get; set; } = string.Empty;

    public string AddressLine { get; set; } = string.Empty;

    public string PostalCode { get; set; } = string.Empty;

    public string City { get; set; } = string.Empty;

    public string Region { get; set; } = string.Empty;

    public string CountryCode { get; set; } = string.Empty;

    public string? Phone { get; set; }

    public string? Email { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public byte[] RowVersion { get; set; } = [];
}
