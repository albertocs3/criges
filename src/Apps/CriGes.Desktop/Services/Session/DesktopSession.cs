using CriGes.Modules.Platform.Contracts.Auth;

namespace CriGes.Desktop.Services.Session;

public sealed class DesktopSession
{
    public string AccessToken { get; private set; } = string.Empty;

    public DateTimeOffset AccessTokenExpiresAtUtc { get; private set; }

    public string RefreshToken { get; private set; } = string.Empty;

    public DateTimeOffset RefreshTokenExpiresAtUtc { get; private set; }

    public Guid SessionId { get; private set; }

    public string DisplayName { get; private set; } = string.Empty;

    public string UserName { get; private set; } = string.Empty;

    public string Role { get; private set; } = string.Empty;

    public IReadOnlyList<string> Permissions { get; private set; } = [];

    public bool IsAuthenticated => SessionId != Guid.Empty && !string.IsNullOrWhiteSpace(AccessToken);

    public void Apply(SessionResponse response)
    {
        AccessToken = response.AccessToken;
        AccessTokenExpiresAtUtc = response.AccessTokenExpiresAtUtc;
        RefreshToken = response.RefreshToken;
        RefreshTokenExpiresAtUtc = response.RefreshTokenExpiresAtUtc;
        SessionId = response.Session.Id;
        DisplayName = response.User.DisplayName;
        UserName = response.User.UserName;
        Role = response.User.Role;
        Permissions = response.User.Permissions;
    }

    public void Clear()
    {
        AccessToken = string.Empty;
        RefreshToken = string.Empty;
        SessionId = Guid.Empty;
        DisplayName = string.Empty;
        UserName = string.Empty;
        Role = string.Empty;
        Permissions = [];
        AccessTokenExpiresAtUtc = default;
        RefreshTokenExpiresAtUtc = default;
    }
}
