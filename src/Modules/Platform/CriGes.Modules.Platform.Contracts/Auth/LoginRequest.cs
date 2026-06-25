namespace CriGes.Modules.Platform.Contracts.Auth;

public sealed record LoginRequest(
    string? UserName,
    string? Password,
    string? DeviceId,
    string? ClientVersion);
