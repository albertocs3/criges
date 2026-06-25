namespace CriGes.Modules.Platform.Contracts.Administration;

public sealed record RolePermissionsResponse(
    Guid RoleId,
    IReadOnlyList<string> Permissions);
