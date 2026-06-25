namespace CriGes.Modules.Platform.Domain.Initialization;

public sealed record NumberCounter(string Code, long CurrentValue)
{
    public static IReadOnlyList<NumberCounter> InitialCounters { get; } =
    [
        new NumberCounter("GLOBAL.AUDIT", 0),
        new NumberCounter("GLOBAL.NOTIFICATION", 0)
    ];
}
