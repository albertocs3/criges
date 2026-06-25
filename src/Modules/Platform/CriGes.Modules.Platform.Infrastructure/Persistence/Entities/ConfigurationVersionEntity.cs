namespace CriGes.Modules.Platform.Infrastructure.Persistence.Entities;

public sealed class ConfigurationVersionEntity
{
    public Guid ConfigurationVersionId { get; set; }

    public long VersionNumber { get; set; }

    public byte Status { get; set; }

    public string LanguageCode { get; set; } = string.Empty;

    public string CurrencyCode { get; set; } = string.Empty;

    public string TimeZoneId { get; set; } = string.Empty;

    public DateTime CreatedAtUtc { get; set; }

    public Guid? CreatedByUserId { get; set; }

    public DateTime? AppliedAtUtc { get; set; }

    public DateTime? SupersededAtUtc { get; set; }

    public byte[] ConfigurationHash { get; set; } = [];

    public byte[] RowVersion { get; set; } = [];
}
