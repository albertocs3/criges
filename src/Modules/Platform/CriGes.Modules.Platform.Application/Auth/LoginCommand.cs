namespace CriGes.Modules.Platform.Application.Auth;

public sealed record LoginCommand(
    string? UserName,
    string? Password,
    string? DeviceId,
    string? ClientVersion);
