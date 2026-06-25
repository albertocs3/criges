namespace CriGes.Modules.Platform.Infrastructure.Persistence.Entities;

public sealed class NumberCounterEntity
{
    public string Code { get; set; } = string.Empty;

    public long CurrentValue { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public byte[] RowVersion { get; set; } = [];
}
