using CriGes.SharedKernel;

namespace CriGes.Modules.Platform.Application.Auth;

public sealed class GetCurrentUserHandler(IAuthSessionContext sessionContext)
{
    public Result<AuthenticatedSessionSnapshot> Handle()
    {
        if (sessionContext.HasInvalidBearerToken || sessionContext.Session is null)
        {
            return Result.Failure<AuthenticatedSessionSnapshot>(AuthErrors.InvalidToken);
        }

        return Result.Success(sessionContext.Session);
    }
}
