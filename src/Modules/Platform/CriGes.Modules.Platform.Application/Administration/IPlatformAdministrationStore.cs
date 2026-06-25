namespace CriGes.Modules.Platform.Application.Administration;

public interface IPlatformAdministrationStore
{
    Task<IReadOnlyList<RoleSummary>> ListRolesAsync(CancellationToken cancellationToken);

    Task<IReadOnlyList<UserSummary>> ListUsersAsync(CancellationToken cancellationToken);

    Task<IReadOnlyList<AuditEventSummary>> ListAuditEventsAsync(
        int take,
        CancellationToken cancellationToken);

    Task<RoleSummary?> FindRoleAsync(Guid roleId, CancellationToken cancellationToken);

    Task<bool> IsRoleNameReservedAsync(string normalizedName, CancellationToken cancellationToken);

    Task<bool> IsUserNameReservedAsync(string normalizedUserName, CancellationToken cancellationToken);

    Task<RoleSummary> CreateRoleAsync(
        RoleCreationData role,
        Guid? actorUserId,
        Guid correlationId,
        CancellationToken cancellationToken);

    Task<UserSummary> CreateUserAsync(
        UserCreationData user,
        Guid? actorUserId,
        Guid correlationId,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<string>> ReplaceRolePermissionsAsync(
        Guid roleId,
        IReadOnlyList<string> permissions,
        Guid? actorUserId,
        DateTimeOffset grantedAtUtc,
        Guid correlationId,
        CancellationToken cancellationToken);
}
