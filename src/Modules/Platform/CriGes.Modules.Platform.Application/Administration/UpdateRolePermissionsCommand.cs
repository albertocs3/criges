namespace CriGes.Modules.Platform.Application.Administration;

public sealed record UpdateRolePermissionsCommand(
    Guid RoleId,
    IReadOnlyList<string>? Permissions);
