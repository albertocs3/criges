namespace CriGes.Modules.Platform.Contracts.Auth;

public sealed record CloseActiveSessionsRequest(
    string? UserName);

