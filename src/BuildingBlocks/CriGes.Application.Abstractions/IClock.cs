namespace CriGes.Application.Abstractions;

public interface IClock
{
    DateTimeOffset UtcNow { get; }
}
