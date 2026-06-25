namespace CriGes.Application.Abstractions;

public interface ICorrelationContext
{
    Guid CorrelationId { get; set; }
}
