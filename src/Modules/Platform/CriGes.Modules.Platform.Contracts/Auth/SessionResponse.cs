namespace CriGes.Modules.Platform.Contracts.Auth;

public sealed record SessionResponse(
    string AccessToken,
    DateTimeOffset AccessTokenExpiresAtUtc,
    string RefreshToken,
    DateTimeOffset RefreshTokenExpiresAtUtc,
    AuthSessionResponse Session,
    AuthUserResponse User);

public sealed record AuthSessionResponse(
    Guid Id,
    DateTimeOffset StartedAtUtc,
    DateTimeOffset IdleExpiresAtUtc);

public sealed record AuthUserResponse(
    Guid Id,
    string DisplayName,
    string UserName,
    string Role,
    IReadOnlyList<string> Permissions);
