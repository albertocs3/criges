namespace CriGes.Modules.Platform.Application.Auth;

internal static class AuthSessionResultFactory
{
    public static LoginResult Create(
        AuthenticatedSessionSnapshot session,
        string accessToken,
        DateTimeOffset accessTokenExpiresAtUtc,
        string refreshToken,
        DateTimeOffset refreshTokenExpiresAtUtc)
    {
        return new LoginResult(
            accessToken,
            accessTokenExpiresAtUtc,
            refreshToken,
            refreshTokenExpiresAtUtc,
            session.SessionId,
            session.StartedAtUtc,
            session.IdleExpiresAtUtc,
            session.UserId,
            session.DisplayName,
            session.UserName,
            session.RoleName,
            session.Permissions);
    }
}
