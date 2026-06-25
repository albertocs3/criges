namespace CriGes.Modules.Platform.Application.Auth;

public sealed record AuthSessionSnapshot(
    Guid SessionId,
    Guid UserId,
    string AccessToken,
    string RefreshToken,
    string RefreshTokenHash,
    DateTimeOffset StartedAtUtc,
    DateTimeOffset LastActivityAtUtc,
    DateTimeOffset IdleExpiresAtUtc,
    DateTimeOffset AccessTokenExpiresAtUtc,
    DateTimeOffset RefreshTokenExpiresAtUtc,
    string DeviceId,
    string ClientVersion,
    string? IpAddress,
    string? UserAgent,
    long SecurityVersion);
