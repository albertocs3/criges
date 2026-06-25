namespace CriGes.Modules.Platform.Domain.Initialization;

public sealed record SystemIdentity(Guid UserId, DateTimeOffset CreatedAtUtc, string NormalizedUserName)
{
    public const string UserName = "Sistema";

    public static SystemIdentity Create(Guid userId, DateTimeOffset createdAtUtc)
    {
        return new SystemIdentity(userId, createdAtUtc, UserName.ToUpperInvariant());
    }
}
