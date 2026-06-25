using System.Security.Cryptography;
using System.Text;
using CriGes.Modules.Platform.Application.Auth;

namespace CriGes.Modules.Platform.Infrastructure.Auth;

public sealed class CryptoAuthSessionTokenGenerator : IAuthSessionTokenGenerator
{
    public string GenerateToken()
    {
        return Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
    }

    public string HashToken(string token)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        return Convert.ToBase64String(hash);
    }
}
