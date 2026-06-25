namespace CriGes.Modules.Platform.Application.Auth;

public interface IAuthSessionContext
{
    AuthenticatedSessionSnapshot? Session { get; set; }

    bool HasInvalidBearerToken { get; set; }

    bool IsAuthenticated => Session is not null;
}
