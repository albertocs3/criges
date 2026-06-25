namespace CriGes.Modules.Platform.Infrastructure.Persistence.Entities;

public sealed class HttpIdempotencyRecordEntity
{
    public string Key { get; set; } = string.Empty;

    public string RequestHash { get; set; } = string.Empty;

    public int? StatusCode { get; set; }

    public string? ResponseJson { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public DateTime? CompletedAtUtc { get; set; }
}
