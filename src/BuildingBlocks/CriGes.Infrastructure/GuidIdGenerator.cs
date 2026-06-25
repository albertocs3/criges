using CriGes.Application.Abstractions;

namespace CriGes.Infrastructure;

public sealed class GuidIdGenerator : IIdGenerator
{
    public Guid NewId()
    {
        return Guid.NewGuid();
    }
}
