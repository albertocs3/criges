namespace CriGes.Modules.Platform.Infrastructure.Persistence.Entities;

public sealed class ReservedUserNameEntity
{
    public string NormalizedUserName { get; set; } = string.Empty;

    public Guid FirstUserId { get; set; }

    public DateTime ReservedAtUtc { get; set; }
}
