namespace CriGes.Modules.Platform.Application.Auth;

public interface IAuthSessionStore
{
    Task<bool> IsPlatformInitializedAsync(CancellationToken cancellationToken);

    Task<AuthUserSnapshot?> FindUserWithRoleAsync(string normalizedUserName, CancellationToken cancellationToken);

    Task<bool> HasActiveSessionAsync(Guid userId, DateTimeOffset now, CancellationToken cancellationToken);

    Task SaveSuccessfulLoginAsync(AuthSessionSnapshot session, CancellationToken cancellationToken);

    Task RecordFailedLoginAsync(Guid userId, short failedLoginCount, DateTimeOffset now, CancellationToken cancellationToken);

    Task<AuthenticatedSessionSnapshot?> FindActiveSessionByAccessTokenAsync(
        string accessToken,
        DateTimeOffset now,
        CancellationToken cancellationToken);

    Task<AuthenticatedSessionSnapshot?> FindActiveSessionByRefreshTokenAsync(
        Guid sessionId,
        string refreshTokenHash,
        DateTimeOffset now,
        CancellationToken cancellationToken);

    Task TouchSessionAsync(Guid sessionId, DateTimeOffset idleExpiresAtUtc, DateTimeOffset now, CancellationToken cancellationToken);

    Task RotateSessionTokensAsync(
        Guid sessionId,
        string accessToken,
        DateTimeOffset accessTokenExpiresAtUtc,
        string refreshTokenHash,
        DateTimeOffset refreshTokenExpiresAtUtc,
        DateTimeOffset idleExpiresAtUtc,
        DateTimeOffset now,
        CancellationToken cancellationToken);

    Task CloseSessionAsync(Guid sessionId, DateTimeOffset now, CancellationToken cancellationToken);
}
