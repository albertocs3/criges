namespace CriGes.Modules.Platform.Contracts.Administration;

public sealed record AuditEventResponse(
    long Id,
    DateTimeOffset OccurredAtUtc,
    Guid? ActorUserId,
    string? ActorDisplayName,
    string Module,
    string Action,
    string? EntityType,
    string? EntityId,
    string Description,
    Guid CorrelationId,
    string Result);
