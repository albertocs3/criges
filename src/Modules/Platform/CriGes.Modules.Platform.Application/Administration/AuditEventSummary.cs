namespace CriGes.Modules.Platform.Application.Administration;

public sealed record AuditEventSummary(
    long Id,
    DateTime OccurredAtUtc,
    Guid? ActorUserId,
    string? ActorDisplayName,
    string Module,
    string Action,
    string? EntityType,
    string? EntityId,
    byte Result,
    string Description,
    Guid CorrelationId);
