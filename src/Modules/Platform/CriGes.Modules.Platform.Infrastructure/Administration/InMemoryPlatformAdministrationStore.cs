using CriGes.Modules.Platform.Application.Administration;
using CriGes.Modules.Platform.Application.Auth;
using CriGes.Modules.Platform.Domain.Initialization;
using CriGes.Modules.Platform.Infrastructure.Initialization;

namespace CriGes.Modules.Platform.Infrastructure.Administration;

public sealed class InMemoryPlatformAdministrationStore(InMemoryPlatformInitializationStore initializationStore) : IPlatformAdministrationStore
{
    private readonly object syncRoot = new();
    private readonly List<UserSummary> users = [];
    private readonly List<AuditEventSummary> auditEvents = [];
    private readonly Dictionary<Guid, IReadOnlyList<string>> rolePermissions = [];
    private long nextAuditEventId;

    public Task<IReadOnlyList<RoleSummary>> ListRolesAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var snapshot = initializationStore.GetSnapshot();
        if (snapshot is null)
        {
            return Task.FromResult<IReadOnlyList<RoleSummary>>([]);
        }

        lock (syncRoot)
        {
            return Task.FromResult<IReadOnlyList<RoleSummary>>(snapshot.BaseRoles
                .OrderBy(role => role.Name)
                .Select(role => new RoleSummary(
                    role.RoleId,
                    role.Name,
                    RoleType: 1,
                    Status: 1,
                    GetPermissions(role.RoleId, role.Name)))
                .ToArray());
        }
    }

    public Task<IReadOnlyList<UserSummary>> ListUsersAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var snapshot = initializationStore.GetSnapshot();
        if (snapshot is null)
        {
            return Task.FromResult<IReadOnlyList<UserSummary>>([]);
        }

        var administratorRole = snapshot.BaseRoles.Single(role => role.Name == "Administrador");

        lock (syncRoot)
        {
            IReadOnlyList<UserSummary> snapshotUsers =
            [
                new UserSummary(
                    snapshot.Administrator.UserId,
                    snapshot.Administrator.FullName,
                    snapshot.Administrator.UserName.Value,
                    Phone: null,
                    administratorRole.RoleId,
                    administratorRole.Name,
                    Status: 1,
                    LastSuccessfulLoginUtc: null,
                    BlockedUntilUtc: null),
                new UserSummary(
                    snapshot.SystemIdentity.UserId,
                    SystemIdentity.UserName,
                    SystemIdentity.UserName,
                    Phone: null,
                    administratorRole.RoleId,
                    administratorRole.Name,
                    Status: 1,
                    LastSuccessfulLoginUtc: null,
                    BlockedUntilUtc: null)
            ];

            return Task.FromResult<IReadOnlyList<UserSummary>>(snapshotUsers
                .Concat(users)
                .OrderBy(user => user.UserName)
                .ToArray());
        }
    }

    public async Task<RoleSummary?> FindRoleAsync(Guid roleId, CancellationToken cancellationToken)
    {
        return (await ListRolesAsync(cancellationToken).ConfigureAwait(false))
            .SingleOrDefault(role => role.Id == roleId);
    }

    public Task<IReadOnlyList<AuditEventSummary>> ListAuditEventsAsync(
        int take,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        lock (syncRoot)
        {
            return Task.FromResult<IReadOnlyList<AuditEventSummary>>(auditEvents
                .OrderByDescending(audit => audit.OccurredAtUtc)
                .ThenByDescending(audit => audit.Id)
                .Take(take)
                .ToArray());
        }
    }

    public Task<bool> IsUserNameReservedAsync(string normalizedUserName, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var snapshot = initializationStore.GetSnapshot();
        var reserved = snapshot?.Administrator.UserName.NormalizedValue == normalizedUserName ||
            snapshot?.SystemIdentity.NormalizedUserName == normalizedUserName;

        lock (syncRoot)
        {
            return Task.FromResult(reserved || users.Any(user => user.UserName.Equals(normalizedUserName, StringComparison.OrdinalIgnoreCase)));
        }
    }

    public async Task<UserSummary> CreateUserAsync(
        UserCreationData user,
        Guid? actorUserId,
        Guid correlationId,
        CancellationToken cancellationToken)
    {
        var role = await FindRoleAsync(user.RoleId, cancellationToken).ConfigureAwait(false)
            ?? throw new InvalidOperationException("Role not found.");
        var summary = new UserSummary(
            user.UserId,
            user.FullName,
            user.UserName,
            user.Phone,
            role.Id,
            role.Name,
            Status: 1,
            LastSuccessfulLoginUtc: null,
            BlockedUntilUtc: null);

        lock (syncRoot)
        {
            users.Add(summary);
            auditEvents.Add(CreateAuditEvent(
                user.CreatedAtUtc.UtcDateTime,
                actorUserId,
                "UserCreated",
                "User",
                user.UserId.ToString("D"),
                $"User '{user.UserName}' created.",
                correlationId));
        }

        return summary;
    }

    public Task<IReadOnlyList<string>> ReplaceRolePermissionsAsync(
        Guid roleId,
        IReadOnlyList<string> permissions,
        Guid? actorUserId,
        DateTimeOffset grantedAtUtc,
        Guid correlationId,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        lock (syncRoot)
        {
            rolePermissions[roleId] = permissions.ToArray();
            auditEvents.Add(CreateAuditEvent(
                grantedAtUtc.UtcDateTime,
                actorUserId,
                "RolePermissionsUpdated",
                "Role",
                roleId.ToString("D"),
                "Role permissions updated.",
                correlationId));
            return Task.FromResult(rolePermissions[roleId]);
        }
    }

    private IReadOnlyList<string> GetPermissions(Guid roleId, string roleName)
    {
        return rolePermissions.TryGetValue(roleId, out var permissions)
            ? permissions
            : RolePermissionCatalog.GetPermissionsForRole(roleName);
    }

    private AuditEventSummary CreateAuditEvent(
        DateTime occurredAtUtc,
        Guid? actorUserId,
        string action,
        string entityType,
        string entityId,
        string description,
        Guid correlationId)
    {
        nextAuditEventId++;
        return new AuditEventSummary(
            nextAuditEventId,
            occurredAtUtc,
            actorUserId,
            actorUserId is null ? null : "Administrador",
            "Platform",
            action,
            entityType,
            entityId,
            Result: 1,
            description,
            correlationId);
    }
}
