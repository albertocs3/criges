namespace CriGes.Modules.Platform.Contracts.Administration;

public sealed record RoleSummaryResponse(
    Guid Id,
    string Name,
    string Type,
    string Status,
    IReadOnlyList<string> Permissions);
