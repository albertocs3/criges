namespace CriGes.Modules.Platform.Application.Administration;

public sealed record RoleCreationData(
    Guid RoleId,
    string Name,
    string NormalizedName,
    DateTimeOffset CreatedAtUtc);
