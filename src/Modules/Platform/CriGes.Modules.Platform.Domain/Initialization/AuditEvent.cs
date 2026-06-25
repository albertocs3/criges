namespace CriGes.Modules.Platform.Domain.Initialization;

public sealed record AuditEvent(Guid AuditEventId, string EventType, DateTimeOffset OccurredAtUtc)
{
    public static AuditEvent PlatformInitialized(Guid auditEventId, DateTimeOffset occurredAtUtc)
    {
        return new AuditEvent(auditEventId, "PlatformInitialized", occurredAtUtc);
    }
}
