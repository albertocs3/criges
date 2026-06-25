namespace CriGes.Modules.Platform.Contracts.Administration;

public sealed record CreateUserRequest(
    string? FullName,
    string? UserName,
    string? Phone,
    Guid RoleId,
    string? Password);
