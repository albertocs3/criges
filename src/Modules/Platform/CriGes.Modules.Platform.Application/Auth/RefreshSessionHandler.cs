using CriGes.Application.Abstractions;
using CriGes.SharedKernel;

namespace CriGes.Modules.Platform.Application.Auth;

public sealed class RefreshSessionHandler(
    IAuthSessionStore sessionStore,
    IAuthSessionTokenGenerator tokenGenerator,
    IClock clock)
{
    private static readonly TimeSpan AccessTokenLifetime = TimeSpan.FromMinutes(15);
    private static readonly TimeSpan RefreshTokenLifetime = TimeSpan.FromDays(30);
    private static readonly TimeSpan IdleLifetime = TimeSpan.FromHours(5);

    public async Task<Result<LoginResult>> HandleAsync(
        RefreshSessionCommand command,
        CancellationToken cancellationToken)
    {
        if (command.SessionId == Guid.Empty || string.IsNullOrWhiteSpace(command.RefreshToken))
        {
            return Result.Failure<LoginResult>(AuthErrors.InvalidToken);
        }

        var now = clock.UtcNow;
        var refreshTokenHash = tokenGenerator.HashToken(command.RefreshToken);
        var session = await sessionStore
            .FindActiveSessionByRefreshTokenAsync(command.SessionId, refreshTokenHash, now, cancellationToken)
            .ConfigureAwait(false);

        if (session is null)
        {
            return Result.Failure<LoginResult>(AuthErrors.InvalidToken);
        }

        if (session.UserStatus != 1)
        {
            return Result.Failure<LoginResult>(AuthErrors.AccountDisabled);
        }

        if (session.RoleStatus != 1)
        {
            return Result.Failure<LoginResult>(AuthErrors.RoleDisabled);
        }

        var newAccessToken = tokenGenerator.GenerateToken();
        var newRefreshToken = tokenGenerator.GenerateToken();
        var refreshedSession = session with
        {
            IdleExpiresAtUtc = now.Add(IdleLifetime),
            RefreshTokenExpiresAtUtc = now.Add(RefreshTokenLifetime)
        };

        await sessionStore
            .RotateSessionTokensAsync(
                session.SessionId,
                newAccessToken,
                now.Add(AccessTokenLifetime),
                tokenGenerator.HashToken(newRefreshToken),
                refreshedSession.RefreshTokenExpiresAtUtc,
                refreshedSession.IdleExpiresAtUtc,
                now,
                cancellationToken)
            .ConfigureAwait(false);

        return Result.Success(AuthSessionResultFactory.Create(
            refreshedSession,
            newAccessToken,
            now.Add(AccessTokenLifetime),
            newRefreshToken,
            refreshedSession.RefreshTokenExpiresAtUtc));
    }
}
