namespace CriGes.Modules.Platform.Infrastructure.Persistence.Entities;

public sealed class CustomerEntity
{
    public Guid CustomerId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string NormalizedName { get; set; } = string.Empty;

    public string? TaxId { get; set; }

    public string? NormalizedTaxId { get; set; }

    public string? Email { get; set; }

    public string? Phone { get; set; }

    public byte Status { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public DateTime ModifiedAtUtc { get; set; }

    public Guid? CreatedByUserId { get; set; }

    public Guid? ModifiedByUserId { get; set; }

    public byte[] RowVersion { get; set; } = [];
}
