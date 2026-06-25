namespace CriGes.Modules.Platform.Application.Auth;

public sealed class AuthSessionContext : IAuthSessionContext
{
    public AuthenticatedSessionSnapshot? Session { get; set; }

    public bool HasInvalidBearerToken { get; set; }
}
