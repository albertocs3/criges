using CriGes.Application.Abstractions;
using CriGes.SharedKernel;

namespace CriGes.Modules.Platform.Application.Auth;

public sealed class LogoutHandler(
    IAuthSessionContext sessionContext,
    IAuthSessionStore sessionStore,
    IClock clock)
{
    public async Task<Result> HandleAsync(CancellationToken cancellationToken)
    {
        if (sessionContext.HasInvalidBearerToken || sessionContext.Session is null)
        {
            return Result.Failure(AuthErrors.InvalidToken);
        }

        await sessionStore.CloseSessionAsync(sessionContext.Session.SessionId, clock.UtcNow, cancellationToken)
            .ConfigureAwait(false);

        return Result.Success();
    }
}
