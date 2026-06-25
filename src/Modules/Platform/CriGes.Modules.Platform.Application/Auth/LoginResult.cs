namespace CriGes.Modules.Platform.Application.Auth;

public sealed record LoginResult(
    string AccessToken,
    DateTimeOffset AccessTokenExpiresAtUtc,
    string RefreshToken,
    DateTimeOffset RefreshTokenExpiresAtUtc,
    Guid SessionId,
    DateTimeOffset StartedAtUtc,
    DateTimeOffset IdleExpiresAtUtc,
    Guid UserId,
    string DisplayName,
    string UserName,
    string Role,
    IReadOnlyList<string> Permissions);
