namespace CriGes.Modules.Platform.Domain.Initialization;

public sealed record InitialAdministrator(
    Guid UserId,
    string FullName,
    UserName UserName,
    string PasswordHash,
    Guid AdministratorRoleId,
    DateTimeOffset CreatedAtUtc);
