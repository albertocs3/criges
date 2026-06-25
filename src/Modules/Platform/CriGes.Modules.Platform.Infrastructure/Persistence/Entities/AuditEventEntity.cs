namespace CriGes.Modules.Platform.Infrastructure.Persistence.Entities;

public sealed class AuditEventEntity
{
    public long AuditEventId { get; set; }

    public DateTime OccurredAtUtc { get; set; }

    public byte ActorType { get; set; }

    public Guid? ActorUserId { get; set; }

    public string? ActorDisplayName { get; set; }

    public string Module { get; set; } = string.Empty;

    public string Action { get; set; } = string.Empty;

    public string? EntityType { get; set; }

    public string? EntityId { get; set; }

    public byte Result { get; set; }

    public string? PreviousValuesJson { get; set; }

    public string? NewValuesJson { get; set; }

    public string Description { get; set; } = string.Empty;

    public Guid CorrelationId { get; set; }

    public string? CreatedByNode { get; set; }
}
