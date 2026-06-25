namespace CriGes.Modules.Platform.Contracts.Auth;

public sealed record CurrentUserResponse(
    Guid Id,
    string DisplayName,
    string UserName,
    CurrentUserRoleResponse Role,
    IReadOnlyList<string> Permissions,
    CurrentUserSessionResponse Session);

public sealed record CurrentUserRoleResponse(
    Guid Id,
    string Name);

public sealed record CurrentUserSessionResponse(
    Guid Id,
    DateTimeOffset IdleExpiresAtUtc);
