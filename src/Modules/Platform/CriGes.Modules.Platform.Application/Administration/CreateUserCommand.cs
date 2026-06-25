namespace CriGes.Modules.Platform.Application.Administration;

public sealed record CreateUserCommand(
    string? FullName,
    string? UserName,
    string? Phone,
    Guid RoleId,
    string? Password);
