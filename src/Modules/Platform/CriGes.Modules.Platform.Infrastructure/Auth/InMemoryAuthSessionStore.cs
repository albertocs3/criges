using CriGes.Modules.Platform.Application.Auth;
using CriGes.Modules.Platform.Infrastructure.Initialization;

namespace CriGes.Modules.Platform.Infrastructure.Auth;

public sealed class InMemoryAuthSessionStore(InMemoryPlatformInitializationStore initializationStore) : IAuthSessionStore
{
    private readonly object syncRoot = new();
    private readonly List<AuthSessionSnapshot> sessions = [];
    private readonly Dictionary<Guid, AuthenticatedSessionSnapshot> authenticatedSessions = [];
    private readonly Dictionary<Guid, short> failedCounts = [];

    public Task<bool> IsPlatformInitializedAsync(CancellationToken cancellationToken)
    {
        return initializationStore.IsInitializedAsync(cancellationToken);
    }

    public Task<AuthUserSnapshot?> FindUserWithRoleAsync(string normalizedUserName, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var snapshot = initializationStore.GetSnapshot();
        if (snapshot is null ||
            snapshot.Administrator.UserName.NormalizedValue != normalizedUserName)
        {
            return Task.FromResult<AuthUserSnapshot?>(null);
        }

        var role = snapshot.BaseRoles.Single(value => value.RoleId == snapshot.Administrator.AdministratorRoleId);
        failedCounts.TryGetValue(snapshot.Administrator.UserId, out var failedLoginCount);

        return Task.FromResult<AuthUserSnapshot?>(new AuthUserSnapshot(
            snapshot.Administrator.UserId,
            snapshot.Administrator.FullName,
            snapshot.Administrator.UserName.Value,
            snapshot.Administrator.UserName.NormalizedValue,
            Status: 1,
            snapshot.Administrator.PasswordHash,
            failedLoginCount,
            BlockedUntilUtc: null,
            SecurityVersion: 1,
            role.RoleId,
            role.Name,
            RoleStatus: 1,
            RolePermissionCatalog.GetPermissionsForRole(role.Name)));
    }

    public Task<bool> HasActiveSessionAsync(Guid userId, DateTimeOffset now, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        lock (syncRoot)
        {
            return Task.FromResult(sessions.Any(session =>
                session.UserId == userId &&
                session.IdleExpiresAtUtc > now &&
                session.RefreshTokenExpiresAtUtc > now));
        }
    }

    public Task SaveSuccessfulLoginAsync(AuthSessionSnapshot session, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        lock (syncRoot)
        {
            sessions.Add(session);
            var snapshot = initializationStore.GetSnapshot();
            var administrator = snapshot?.Administrator;
            var role = snapshot?.BaseRoles.Single(value => value.RoleId == administrator?.AdministratorRoleId);
            if (administrator is not null && role is not null)
            {
                authenticatedSessions[session.SessionId] = new AuthenticatedSessionSnapshot(
                    session.SessionId,
                    session.UserId,
                    administrator.FullName,
                    administrator.UserName.Value,
                    role.RoleId,
                    role.Name,
                    UserStatus: 1,
                    RoleStatus: 1,
                    session.SecurityVersion,
                    session.StartedAtUtc,
                    session.IdleExpiresAtUtc,
                    session.RefreshTokenExpiresAtUtc,
                    RolePermissionCatalog.GetPermissionsForRole(role.Name));
            }
            failedCounts[session.UserId] = 0;
        }

        return Task.CompletedTask;
    }

    public Task RecordFailedLoginAsync(Guid userId, short failedLoginCount, DateTimeOffset now, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        lock (syncRoot)
        {
            failedCounts[userId] = failedLoginCount;
        }

        return Task.CompletedTask;
    }

    public Task<AuthenticatedSessionSnapshot?> FindActiveSessionByAccessTokenAsync(
        string accessToken,
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        lock (syncRoot)
        {
            var session = sessions.SingleOrDefault(value =>
                value.AccessToken == accessToken &&
                value.IdleExpiresAtUtc > now &&
                value.RefreshTokenExpiresAtUtc > now);

            return Task.FromResult(session is null ? null : authenticatedSessions[session.SessionId]);
        }
    }

    public Task<AuthenticatedSessionSnapshot?> FindActiveSessionByRefreshTokenAsync(
        Guid sessionId,
        string refreshTokenHash,
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        lock (syncRoot)
        {
            var session = sessions.SingleOrDefault(value =>
                value.SessionId == sessionId &&
                value.RefreshTokenHash == refreshTokenHash &&
                value.IdleExpiresAtUtc > now &&
                value.RefreshTokenExpiresAtUtc > now);

            return Task.FromResult(session is null ? null : authenticatedSessions[session.SessionId]);
        }
    }

    public Task TouchSessionAsync(Guid sessionId, DateTimeOffset idleExpiresAtUtc, DateTimeOffset now, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        lock (syncRoot)
        {
            UpdateSession(sessionId, session => session with { LastActivityAtUtc = now, IdleExpiresAtUtc = idleExpiresAtUtc });
            authenticatedSessions[sessionId] = authenticatedSessions[sessionId] with { IdleExpiresAtUtc = idleExpiresAtUtc };
        }

        return Task.CompletedTask;
    }

    public Task RotateSessionTokensAsync(
        Guid sessionId,
        string accessToken,
        DateTimeOffset accessTokenExpiresAtUtc,
        string refreshTokenHash,
        DateTimeOffset refreshTokenExpiresAtUtc,
        DateTimeOffset idleExpiresAtUtc,
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        lock (syncRoot)
        {
            UpdateSession(sessionId, session => session with
            {
                AccessToken = accessToken,
                RefreshTokenHash = refreshTokenHash,
                LastActivityAtUtc = now,
                IdleExpiresAtUtc = idleExpiresAtUtc,
                AccessTokenExpiresAtUtc = accessTokenExpiresAtUtc,
                RefreshTokenExpiresAtUtc = refreshTokenExpiresAtUtc
            });
            authenticatedSessions[sessionId] = authenticatedSessions[sessionId] with
            {
                IdleExpiresAtUtc = idleExpiresAtUtc,
                RefreshTokenExpiresAtUtc = refreshTokenExpiresAtUtc
            };
        }

        return Task.CompletedTask;
    }

    public Task CloseSessionAsync(Guid sessionId, DateTimeOffset now, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        lock (syncRoot)
        {
            sessions.RemoveAll(value => value.SessionId == sessionId);
            authenticatedSessions.Remove(sessionId);
        }

        return Task.CompletedTask;
    }

    private void UpdateSession(Guid sessionId, Func<AuthSessionSnapshot, AuthSessionSnapshot> update)
    {
        var index = sessions.FindIndex(value => value.SessionId == sessionId);
        if (index >= 0)
        {
            sessions[index] = update(sessions[index]);
        }
    }
}
