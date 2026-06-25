namespace CriGes.Modules.Platform.Infrastructure.Persistence.Entities;

public sealed class UserSessionEntity
{
    public Guid SessionId { get; set; }

    public Guid UserId { get; set; }

    public byte Status { get; set; }

    public string AccessToken { get; set; } = string.Empty;

    public DateTime AccessTokenExpiresAtUtc { get; set; }

    public string RefreshTokenHash { get; set; } = string.Empty;

    public DateTime RefreshTokenExpiresAtUtc { get; set; }

    public DateTime StartedAtUtc { get; set; }

    public DateTime LastActivityAtUtc { get; set; }

    public DateTime IdleExpiresAtUtc { get; set; }

    public DateTime? ClosedAtUtc { get; set; }

    public string DeviceId { get; set; } = string.Empty;

    public string ClientVersion { get; set; } = string.Empty;

    public string? IpAddress { get; set; }

    public string? UserAgent { get; set; }

    public long SecurityVersion { get; set; }

    public byte[] RowVersion { get; set; } = [];
}
