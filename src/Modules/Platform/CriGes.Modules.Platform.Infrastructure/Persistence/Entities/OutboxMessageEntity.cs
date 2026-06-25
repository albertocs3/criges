namespace CriGes.Modules.Platform.Infrastructure.Persistence.Entities;

public sealed class OutboxMessageEntity
{
    public Guid OutboxMessageId { get; set; }

    public DateTime OccurredAtUtc { get; set; }

    public DateTime AvailableAtUtc { get; set; }

    public string MessageType { get; set; } = string.Empty;

    public int SchemaVersion { get; set; }

    public string PayloadJson { get; set; } = string.Empty;

    public string? HeadersJson { get; set; }

    public Guid CorrelationId { get; set; }

    public string? IdempotencyKey { get; set; }

    public byte Status { get; set; }

    public int AttemptCount { get; set; }

    public DateTime? NextAttemptAtUtc { get; set; }

    public string? LockedBy { get; set; }

    public DateTime? LockExpiresAtUtc { get; set; }

    public DateTime? ProcessedAtUtc { get; set; }

    public string? LastErrorCode { get; set; }

    public string? LastErrorSafeDetail { get; set; }
}
