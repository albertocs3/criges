namespace CriGes.Modules.Platform.Contracts.Administration;

public sealed record UpdateRolePermissionsRequest(IReadOnlyList<string>? Permissions);
