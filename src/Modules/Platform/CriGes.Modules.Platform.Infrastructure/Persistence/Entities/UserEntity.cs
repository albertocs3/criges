namespace CriGes.Modules.Platform.Infrastructure.Persistence.Entities;

public sealed class UserEntity
{
    public Guid UserId { get; set; }

    public string FullName { get; set; } = string.Empty;

    public string UserName { get; set; } = string.Empty;

    public string NormalizedUserName { get; set; } = string.Empty;

    public string? Phone { get; set; }

    public Guid RoleId { get; set; }

    public byte Status { get; set; }

    public string PasswordHash { get; set; } = string.Empty;

    public long SecurityVersion { get; set; }

    public short FailedLoginCount { get; set; }

    public DateTime? BlockedUntilUtc { get; set; }

    public DateTime? LastSuccessfulLoginUtc { get; set; }

    public DateTime PasswordChangedAtUtc { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public DateTime? DeactivatedAtUtc { get; set; }

    public string? DeactivationReason { get; set; }

    public Guid? CreatedByUserId { get; set; }

    public DateTime ModifiedAtUtc { get; set; }

    public Guid? ModifiedByUserId { get; set; }

    public byte[] RowVersion { get; set; } = [];
}
