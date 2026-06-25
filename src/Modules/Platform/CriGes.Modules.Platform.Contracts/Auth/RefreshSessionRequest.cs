namespace CriGes.Modules.Platform.Contracts.Auth;

public sealed record RefreshSessionRequest(
    Guid SessionId,
    string? RefreshToken);
