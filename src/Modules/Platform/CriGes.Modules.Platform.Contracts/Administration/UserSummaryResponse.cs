namespace CriGes.Modules.Platform.Contracts.Administration;

public sealed record UserSummaryResponse(
    Guid Id,
    string FullName,
    string UserName,
    string? Phone,
    RoleReferenceResponse Role,
    string Status,
    DateTimeOffset? LastSuccessfulLoginUtc,
    DateTimeOffset? BlockedUntilUtc);

public sealed record RoleReferenceResponse(Guid Id, string Name);
