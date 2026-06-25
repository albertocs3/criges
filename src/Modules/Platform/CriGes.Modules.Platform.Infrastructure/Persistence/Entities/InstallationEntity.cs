namespace CriGes.Modules.Platform.Infrastructure.Persistence.Entities;

public sealed class InstallationEntity
{
    public Guid InstallationId { get; set; }

    public byte SingletonKey { get; set; } = 1;

    public byte Status { get; set; }

    public DateTime? StartedAtUtc { get; set; }

    public DateTime? CompletedAtUtc { get; set; }

    public Guid? InitialAdministratorUserId { get; set; }

    public string ProductVersion { get; set; } = string.Empty;

    public string? FailureCode { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public byte[] RowVersion { get; set; } = [];
}
