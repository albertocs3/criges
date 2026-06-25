namespace CriGes.Modules.Platform.Application.Administration;

public sealed record UserCreationData(
    Guid UserId,
    string FullName,
    string UserName,
    string NormalizedUserName,
    string? Phone,
    Guid RoleId,
    string PasswordHash,
    DateTimeOffset CreatedAtUtc);
