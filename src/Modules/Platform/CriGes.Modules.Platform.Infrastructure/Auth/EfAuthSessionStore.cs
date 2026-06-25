using CriGes.Modules.Platform.Application.Auth;
using CriGes.Modules.Platform.Domain.Initialization;
using CriGes.Modules.Platform.Infrastructure.Persistence;
using CriGes.Modules.Platform.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace CriGes.Modules.Platform.Infrastructure.Auth;

public sealed class EfAuthSessionStore(PlatformDbContext dbContext) : IAuthSessionStore
{
    public Task<bool> IsPlatformInitializedAsync(CancellationToken cancellationToken)
    {
        return dbContext.Installations.AnyAsync(
            installation => installation.Status == (byte)InstallationStatus.Initialized,
            cancellationToken);
    }

    public async Task<AuthUserSnapshot?> FindUserWithRoleAsync(string normalizedUserName, CancellationToken cancellationToken)
    {
        var user = await dbContext.Users
            .AsNoTracking()
            .Where(user => user.NormalizedUserName == normalizedUserName)
            .Join(
                dbContext.Roles.AsNoTracking(),
                user => user.RoleId,
                role => role.RoleId,
                (user, role) => new AuthUserSnapshot(
                    user.UserId,
                    user.FullName,
                    user.UserName,
                    user.NormalizedUserName,
                    user.Status,
                    user.PasswordHash,
                    user.FailedLoginCount,
                    user.BlockedUntilUtc,
                    user.SecurityVersion,
                    role.RoleId,
                    role.Name,
                    role.Status,
                    Array.Empty<string>()))
            .SingleOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);

        if (user is null)
        {
            return null;
        }

        var permissions = await GetPermissionsAsync(user.RoleId, cancellationToken).ConfigureAwait(false);
        return user with { Permissions = permissions };
    }

    public Task<bool> HasActiveSessionAsync(Guid userId, DateTimeOffset now, CancellationToken cancellationToken)
    {
        var nowUtc = now.UtcDateTime;
        return dbContext.UserSessions.AnyAsync(
            session => session.UserId == userId &&
                session.Status == 1 &&
                session.IdleExpiresAtUtc > nowUtc &&
                session.RefreshTokenExpiresAtUtc > nowUtc,
            cancellationToken);
    }

    public async Task SaveSuccessfulLoginAsync(AuthSessionSnapshot session, CancellationToken cancellationToken)
    {
        await using var transaction = await dbContext.Database
            .BeginTransactionAsync(cancellationToken)
            .ConfigureAwait(false);

        var user = await dbContext.Users
            .SingleAsync(value => value.UserId == session.UserId, cancellationToken)
            .ConfigureAwait(false);

        user.FailedLoginCount = 0;
        user.LastSuccessfulLoginUtc = session.StartedAtUtc.UtcDateTime;
        user.ModifiedAtUtc = session.StartedAtUtc.UtcDateTime;

        dbContext.UserSessions.Add(new UserSessionEntity
        {
            SessionId = session.SessionId,
            UserId = session.UserId,
            Status = 1,
            AccessToken = session.AccessToken,
            AccessTokenExpiresAtUtc = session.AccessTokenExpiresAtUtc.UtcDateTime,
            RefreshTokenHash = session.RefreshTokenHash,
            RefreshTokenExpiresAtUtc = session.RefreshTokenExpiresAtUtc.UtcDateTime,
            StartedAtUtc = session.StartedAtUtc.UtcDateTime,
            LastActivityAtUtc = session.LastActivityAtUtc.UtcDateTime,
            IdleExpiresAtUtc = session.IdleExpiresAtUtc.UtcDateTime,
            DeviceId = session.DeviceId,
            ClientVersion = session.ClientVersion,
            IpAddress = session.IpAddress,
            UserAgent = session.UserAgent,
            SecurityVersion = session.SecurityVersion
        });

        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task RecordFailedLoginAsync(
        Guid userId,
        short failedLoginCount,
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        var user = await dbContext.Users
            .SingleAsync(value => value.UserId == userId, cancellationToken)
            .ConfigureAwait(false);

        user.FailedLoginCount = failedLoginCount;
        user.ModifiedAtUtc = now.UtcDateTime;

        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task<AuthenticatedSessionSnapshot?> FindActiveSessionByAccessTokenAsync(
        string accessToken,
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        var nowUtc = now.UtcDateTime;
        var session = await dbContext.UserSessions
            .AsNoTracking()
            .Where(value => value.Status == 1 &&
                value.IdleExpiresAtUtc > nowUtc &&
                value.RefreshTokenExpiresAtUtc > nowUtc &&
                value.AccessTokenExpiresAtUtc > nowUtc &&
                value.AccessToken == accessToken)
            .SingleOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);

        return await ToAuthenticatedSessionAsync(session, cancellationToken).ConfigureAwait(false);
    }

    public async Task<AuthenticatedSessionSnapshot?> FindActiveSessionByRefreshTokenAsync(
        Guid sessionId,
        string refreshTokenHash,
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        var nowUtc = now.UtcDateTime;
        var session = await dbContext.UserSessions
            .AsNoTracking()
            .Where(value => value.Status == 1 &&
                value.IdleExpiresAtUtc > nowUtc &&
                value.RefreshTokenExpiresAtUtc > nowUtc &&
                value.SessionId == sessionId &&
                value.RefreshTokenHash == refreshTokenHash)
            .SingleOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);

        return await ToAuthenticatedSessionAsync(session, cancellationToken).ConfigureAwait(false);
    }

    public async Task TouchSessionAsync(
        Guid sessionId,
        DateTimeOffset idleExpiresAtUtc,
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        var session = await dbContext.UserSessions
            .SingleAsync(value => value.SessionId == sessionId, cancellationToken)
            .ConfigureAwait(false);

        session.LastActivityAtUtc = now.UtcDateTime;
        session.IdleExpiresAtUtc = idleExpiresAtUtc.UtcDateTime;

        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task RotateSessionTokensAsync(
        Guid sessionId,
        string accessToken,
        DateTimeOffset accessTokenExpiresAtUtc,
        string refreshTokenHash,
        DateTimeOffset refreshTokenExpiresAtUtc,
        DateTimeOffset idleExpiresAtUtc,
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        var session = await dbContext.UserSessions
            .SingleAsync(value => value.SessionId == sessionId, cancellationToken)
            .ConfigureAwait(false);

        session.AccessToken = accessToken;
        session.AccessTokenExpiresAtUtc = accessTokenExpiresAtUtc.UtcDateTime;
        session.RefreshTokenHash = refreshTokenHash;
        session.RefreshTokenExpiresAtUtc = refreshTokenExpiresAtUtc.UtcDateTime;
        session.LastActivityAtUtc = now.UtcDateTime;
        session.IdleExpiresAtUtc = idleExpiresAtUtc.UtcDateTime;

        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task CloseSessionAsync(Guid sessionId, DateTimeOffset now, CancellationToken cancellationToken)
    {
        var session = await dbContext.UserSessions
            .SingleAsync(value => value.SessionId == sessionId, cancellationToken)
            .ConfigureAwait(false);

        session.Status = 2;
        session.ClosedAtUtc = now.UtcDateTime;
        session.LastActivityAtUtc = now.UtcDateTime;

        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task<int> CloseActiveSessionsForUserAsync(
        string normalizedUserName,
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        var nowUtc = now.UtcDateTime;
        var sessions = await dbContext.UserSessions
            .Where(session => session.Status == 1)
            .Join(
                dbContext.Users.Where(user => user.NormalizedUserName == normalizedUserName),
                session => session.UserId,
                user => user.UserId,
                (session, _) => session)
            .ToArrayAsync(cancellationToken)
            .ConfigureAwait(false);

        foreach (var session in sessions)
        {
            session.Status = 2;
            session.ClosedAtUtc = nowUtc;
            session.LastActivityAtUtc = nowUtc;
        }

        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return sessions.Length;
    }

    private async Task<AuthenticatedSessionSnapshot?> ToAuthenticatedSessionAsync(
        UserSessionEntity? session,
        CancellationToken cancellationToken)
    {
        if (session is null)
        {
            return null;
        }

        var userRole = await dbContext.Users
            .AsNoTracking()
            .Where(user => user.UserId == session.UserId)
            .Join(
                dbContext.Roles.AsNoTracking(),
                user => user.RoleId,
                role => role.RoleId,
                (user, role) => new UserRoleSnapshot(
                    user.UserId,
                    user.FullName,
                    user.UserName,
                    user.Status,
                    role.RoleId,
                    role.Name,
                    role.Status))
            .SingleOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);

        if (userRole is null)
        {
            return null;
        }

        var permissions = await GetPermissionsAsync(userRole.RoleId, cancellationToken).ConfigureAwait(false);

        return new AuthenticatedSessionSnapshot(
            session.SessionId,
            userRole.UserId,
            userRole.FullName,
            userRole.UserName,
            userRole.RoleId,
            userRole.RoleName,
            userRole.UserStatus,
            userRole.RoleStatus,
            session.SecurityVersion,
            new DateTimeOffset(session.StartedAtUtc, TimeSpan.Zero),
            new DateTimeOffset(session.IdleExpiresAtUtc, TimeSpan.Zero),
            new DateTimeOffset(session.RefreshTokenExpiresAtUtc, TimeSpan.Zero),
            permissions);
    }

    private async Task<IReadOnlyList<string>> GetPermissionsAsync(Guid roleId, CancellationToken cancellationToken)
    {
        return await dbContext.RolePermissions
            .AsNoTracking()
            .Where(permission => permission.RoleId == roleId)
            .OrderBy(permission => permission.Permission)
            .Select(permission => permission.Permission)
            .ToArrayAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    private sealed record UserRoleSnapshot(
        Guid UserId,
        string FullName,
        string UserName,
        byte UserStatus,
        Guid RoleId,
        string RoleName,
        byte RoleStatus);
}
