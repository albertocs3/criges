namespace CriGes.Modules.Platform.Application.Auth;

public sealed record AuthUserSnapshot(
    Guid UserId,
    string FullName,
    string UserName,
    string NormalizedUserName,
    byte Status,
    string PasswordHash,
    short FailedLoginCount,
    DateTime? BlockedUntilUtc,
    long SecurityVersion,
    Guid RoleId,
    string RoleName,
    byte RoleStatus,
    IReadOnlyList<string> Permissions);
