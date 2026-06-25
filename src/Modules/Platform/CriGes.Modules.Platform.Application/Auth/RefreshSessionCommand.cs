namespace CriGes.Modules.Platform.Application.Auth;

public sealed record RefreshSessionCommand(Guid SessionId, string? RefreshToken);
