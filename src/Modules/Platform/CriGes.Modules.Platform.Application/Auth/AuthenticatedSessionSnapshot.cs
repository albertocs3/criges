namespace CriGes.Modules.Platform.Application.Auth;

public sealed record AuthenticatedSessionSnapshot(
    Guid SessionId,
    Guid UserId,
    string DisplayName,
    string UserName,
    Guid RoleId,
    string RoleName,
    byte UserStatus,
    byte RoleStatus,
    long SecurityVersion,
    DateTimeOffset StartedAtUtc,
    DateTimeOffset IdleExpiresAtUtc,
    DateTimeOffset RefreshTokenExpiresAtUtc,
    IReadOnlyList<string> Permissions);
