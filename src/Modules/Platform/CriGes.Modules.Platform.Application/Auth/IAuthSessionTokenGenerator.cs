namespace CriGes.Modules.Platform.Application.Auth;

public interface IAuthSessionTokenGenerator
{
    string GenerateToken();

    string HashToken(string token);
}
