namespace CriGes.Modules.Platform.Application.Administration;

public sealed record UserSummary(
    Guid Id,
    string FullName,
    string UserName,
    string? Phone,
    Guid RoleId,
    string RoleName,
    byte Status,
    DateTime? LastSuccessfulLoginUtc,
    DateTime? BlockedUntilUtc);
