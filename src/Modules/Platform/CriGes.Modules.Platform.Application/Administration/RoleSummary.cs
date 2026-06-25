namespace CriGes.Modules.Platform.Application.Administration;

public sealed record RoleSummary(
    Guid Id,
    string Name,
    byte RoleType,
    byte Status,
    IReadOnlyList<string> Permissions);
