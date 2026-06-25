using CriGes.Application.Abstractions;

namespace CriGes.Api.Correlation;

public sealed class CorrelationContext : ICorrelationContext
{
    public Guid CorrelationId { get; set; }
}
