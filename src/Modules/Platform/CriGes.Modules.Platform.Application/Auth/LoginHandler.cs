using CriGes.Application.Abstractions;
using CriGes.SharedKernel;

namespace CriGes.Modules.Platform.Application.Auth;

public sealed class LoginHandler(
    IAuthSessionStore sessionStore,
    IAuthSessionTokenGenerator tokenGenerator,
    IPasswordHasher passwordHasher,
    IClock clock)
{
    private static readonly TimeSpan AccessTokenLifetime = TimeSpan.FromMinutes(15);
    private static readonly TimeSpan RefreshTokenLifetime = TimeSpan.FromDays(30);
    private static readonly TimeSpan IdleLifetime = TimeSpan.FromHours(5);

    public async Task<Result<LoginResult>> HandleAsync(
        LoginCommand command,
        string? ipAddress,
        string? userAgent,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(command.UserName) ||
            string.IsNullOrWhiteSpace(command.Password) ||
            string.IsNullOrWhiteSpace(command.DeviceId) ||
            string.IsNullOrWhiteSpace(command.ClientVersion))
        {
            return Result.Failure<LoginResult>(AuthErrors.ValidationFailed);
        }

        if (!await sessionStore.IsPlatformInitializedAsync(cancellationToken).ConfigureAwait(false))
        {
            return Result.Failure<LoginResult>(AuthErrors.PlatformNotInitialized);
        }

        var normalizedUserName = NormalizeUserName(command.UserName);
        var user = await sessionStore
            .FindUserWithRoleAsync(normalizedUserName, cancellationToken)
            .ConfigureAwait(false);

        if (user is null)
        {
            return Result.Failure<LoginResult>(AuthErrors.InvalidCredentials);
        }

        var now = clock.UtcNow;
        if (user.BlockedUntilUtc is not null && user.BlockedUntilUtc.Value > now.UtcDateTime)
        {
            return Result.Failure<LoginResult>(AuthErrors.AccountLocked);
        }

        if (user.Status != 1)
        {
            return Result.Failure<LoginResult>(AuthErrors.AccountDisabled);
        }

        if (user.RoleStatus != 1)
        {
            return Result.Failure<LoginResult>(AuthErrors.RoleDisabled);
        }

        if (!passwordHasher.VerifyPassword(command.Password, user.PasswordHash))
        {
            await sessionStore
                .RecordFailedLoginAsync(user.UserId, (short)(user.FailedLoginCount + 1), now, cancellationToken)
                .ConfigureAwait(false);

            return Result.Failure<LoginResult>(AuthErrors.InvalidCredentials);
        }

        if (await sessionStore.HasActiveSessionAsync(user.UserId, now, cancellationToken).ConfigureAwait(false))
        {
            return Result.Failure<LoginResult>(AuthErrors.ActiveSessionExists);
        }

        var accessToken = tokenGenerator.GenerateToken();
        var refreshToken = tokenGenerator.GenerateToken();
        var sessionId = Guid.NewGuid();
        var session = new AuthSessionSnapshot(
            sessionId,
            user.UserId,
            accessToken,
            refreshToken,
            tokenGenerator.HashToken(refreshToken),
            now,
            now,
            now.Add(IdleLifetime),
            now.Add(AccessTokenLifetime),
            now.Add(RefreshTokenLifetime),
            command.DeviceId.Trim(),
            command.ClientVersion.Trim(),
            ipAddress,
            userAgent,
            user.SecurityVersion);

        await sessionStore.SaveSuccessfulLoginAsync(session, cancellationToken).ConfigureAwait(false);

        return Result.Success(new LoginResult(
            accessToken,
            session.AccessTokenExpiresAtUtc,
            refreshToken,
            session.RefreshTokenExpiresAtUtc,
            session.SessionId,
            session.StartedAtUtc,
            session.IdleExpiresAtUtc,
            user.UserId,
            user.FullName,
            user.UserName,
            user.RoleName,
            user.Permissions));
    }

    private static string NormalizeUserName(string userName)
    {
        return userName.Trim().ToUpperInvariant();
    }
}
