using CriGes.Modules.Platform.Application.Administration;
using CriGes.Modules.Platform.Infrastructure.Persistence;
using CriGes.Modules.Platform.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace CriGes.Modules.Platform.Infrastructure.Administration;

public sealed class EfPlatformAdministrationStore(PlatformDbContext dbContext) : IPlatformAdministrationStore
{
    public async Task<IReadOnlyList<RoleSummary>> ListRolesAsync(CancellationToken cancellationToken)
    {
        var roles = await dbContext.Roles
            .AsNoTracking()
            .OrderBy(role => role.Name)
            .Select(role => new RoleSummary(
                role.RoleId,
                role.Name,
                role.RoleType,
                role.Status,
                Array.Empty<string>()))
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return await PopulatePermissionsAsync(roles, cancellationToken).ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<UserSummary>> ListUsersAsync(CancellationToken cancellationToken)
    {
        return await dbContext.Users
            .AsNoTracking()
            .Join(
                dbContext.Roles.AsNoTracking(),
                user => user.RoleId,
                role => role.RoleId,
                (user, role) => new { User = user, Role = role })
            .OrderBy(value => value.User.UserName)
            .Select(value => new UserSummary(
                value.User.UserId,
                value.User.FullName,
                value.User.UserName,
                value.User.Phone,
                value.Role.RoleId,
                value.Role.Name,
                value.User.Status,
                value.User.LastSuccessfulLoginUtc,
                value.User.BlockedUntilUtc))
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<AuditEventSummary>> ListAuditEventsAsync(
        int take,
        CancellationToken cancellationToken)
    {
        return await dbContext.AuditEvents
            .AsNoTracking()
            .OrderByDescending(audit => audit.OccurredAtUtc)
            .ThenByDescending(audit => audit.AuditEventId)
            .Take(take)
            .Select(audit => new AuditEventSummary(
                audit.AuditEventId,
                audit.OccurredAtUtc,
                audit.ActorUserId,
                audit.ActorDisplayName,
                audit.Module,
                audit.Action,
                audit.EntityType,
                audit.EntityId,
                audit.Result,
                audit.Description,
                audit.CorrelationId))
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<RoleSummary?> FindRoleAsync(Guid roleId, CancellationToken cancellationToken)
    {
        var role = await dbContext.Roles
            .AsNoTracking()
            .Where(value => value.RoleId == roleId)
            .Select(value => new RoleSummary(
                value.RoleId,
                value.Name,
                value.RoleType,
                value.Status,
                Array.Empty<string>()))
            .SingleOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);

        if (role is null)
        {
            return null;
        }

        var permissions = await dbContext.RolePermissions
            .AsNoTracking()
            .Where(permission => permission.RoleId == role.Id)
            .OrderBy(permission => permission.Permission)
            .Select(permission => permission.Permission)
            .ToArrayAsync(cancellationToken)
            .ConfigureAwait(false);

        return role with { Permissions = permissions };
    }

    public Task<bool> IsUserNameReservedAsync(string normalizedUserName, CancellationToken cancellationToken)
    {
        return dbContext.ReservedUserNames.AnyAsync(
            reserved => reserved.NormalizedUserName == normalizedUserName,
            cancellationToken);
    }

    public async Task<UserSummary> CreateUserAsync(
        UserCreationData user,
        Guid? actorUserId,
        Guid correlationId,
        CancellationToken cancellationToken)
    {
        await using var transaction = await dbContext.Database
            .BeginTransactionAsync(cancellationToken)
            .ConfigureAwait(false);

        var role = await dbContext.Roles
            .AsNoTracking()
            .SingleAsync(value => value.RoleId == user.RoleId, cancellationToken)
            .ConfigureAwait(false);

        var now = user.CreatedAtUtc.UtcDateTime;
        dbContext.Users.Add(new UserEntity
        {
            UserId = user.UserId,
            FullName = user.FullName,
            UserName = user.UserName,
            NormalizedUserName = user.NormalizedUserName,
            Phone = user.Phone,
            RoleId = user.RoleId,
            Status = 1,
            PasswordHash = user.PasswordHash,
            SecurityVersion = 1,
            PasswordChangedAtUtc = now,
            CreatedAtUtc = now,
            ModifiedAtUtc = now,
            CreatedByUserId = actorUserId,
            ModifiedByUserId = actorUserId
        });
        dbContext.ReservedUserNames.Add(new ReservedUserNameEntity
        {
            NormalizedUserName = user.NormalizedUserName,
            FirstUserId = user.UserId,
            ReservedAtUtc = now
        });
        await AddAuditEventAsync(
                occurredAtUtc: now,
                actorUserId,
                action: "UserCreated",
                entityType: "User",
                entityId: user.UserId.ToString("D"),
                previousValues: null,
                newValues: new
                {
                    user.UserId,
                    user.FullName,
                    user.UserName,
                    user.Phone,
                    RoleId = role.RoleId,
                    RoleName = role.Name,
                    Status = "active"
                },
                description: $"User '{user.UserName}' created.",
                correlationId,
                cancellationToken)
            .ConfigureAwait(false);

        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);

        return new UserSummary(
            user.UserId,
            user.FullName,
            user.UserName,
            user.Phone,
            role.RoleId,
            role.Name,
            Status: 1,
            LastSuccessfulLoginUtc: null,
            BlockedUntilUtc: null);
    }

    public async Task<IReadOnlyList<string>> ReplaceRolePermissionsAsync(
        Guid roleId,
        IReadOnlyList<string> permissions,
        Guid? actorUserId,
        DateTimeOffset grantedAtUtc,
        Guid correlationId,
        CancellationToken cancellationToken)
    {
        await using var transaction = await dbContext.Database
            .BeginTransactionAsync(cancellationToken)
            .ConfigureAwait(false);

        var currentPermissions = await dbContext.RolePermissions
            .Where(permission => permission.RoleId == roleId)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
        var role = await dbContext.Roles
            .AsNoTracking()
            .SingleAsync(value => value.RoleId == roleId, cancellationToken)
            .ConfigureAwait(false);
        var previousPermissions = currentPermissions
            .Select(permission => permission.Permission)
            .OrderBy(permission => permission, StringComparer.Ordinal)
            .ToArray();

        dbContext.RolePermissions.RemoveRange(currentPermissions);

        foreach (var permission in permissions)
        {
            dbContext.RolePermissions.Add(new RolePermissionEntity
            {
                RoleId = roleId,
                Permission = permission,
                GrantedAtUtc = grantedAtUtc.UtcDateTime,
                GrantedByUserId = actorUserId
            });
        }
        await AddAuditEventAsync(
                occurredAtUtc: grantedAtUtc.UtcDateTime,
                actorUserId,
                action: "RolePermissionsUpdated",
                entityType: "Role",
                entityId: roleId.ToString("D"),
                previousValues: new { RoleId = role.RoleId, RoleName = role.Name, Permissions = previousPermissions },
                newValues: new { RoleId = role.RoleId, RoleName = role.Name, Permissions = permissions },
                description: $"Permissions updated for role '{role.Name}'.",
                correlationId,
                cancellationToken)
            .ConfigureAwait(false);

        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);

        return permissions.ToArray();
    }

    private async Task AddAuditEventAsync(
        DateTime occurredAtUtc,
        Guid? actorUserId,
        string action,
        string entityType,
        string entityId,
        object? previousValues,
        object? newValues,
        string description,
        Guid correlationId,
        CancellationToken cancellationToken)
    {
        var actorDisplayName = actorUserId is null
            ? null
            : await dbContext.Users
                .AsNoTracking()
                .Where(user => user.UserId == actorUserId)
                .Select(user => user.FullName)
                .SingleOrDefaultAsync(cancellationToken)
                .ConfigureAwait(false);

        dbContext.AuditEvents.Add(new AuditEventEntity
        {
            OccurredAtUtc = occurredAtUtc,
            ActorType = actorUserId is null ? (byte)2 : (byte)1,
            ActorUserId = actorUserId,
            ActorDisplayName = actorDisplayName,
            Module = "Platform",
            Action = action,
            EntityType = entityType,
            EntityId = entityId,
            Result = 1,
            PreviousValuesJson = previousValues is null ? null : JsonSerializer.Serialize(previousValues),
            NewValuesJson = newValues is null ? null : JsonSerializer.Serialize(newValues),
            Description = description,
            CorrelationId = correlationId,
            CreatedByNode = "CriGes.Api"
        });
    }

    private async Task<IReadOnlyList<RoleSummary>> PopulatePermissionsAsync(
        IReadOnlyList<RoleSummary> roles,
        CancellationToken cancellationToken)
    {
        var roleIds = roles.Select(role => role.Id).ToArray();
        var permissions = await dbContext.RolePermissions
            .AsNoTracking()
            .Where(permission => roleIds.Contains(permission.RoleId))
            .OrderBy(permission => permission.Permission)
            .GroupBy(permission => permission.RoleId)
            .Select(group => new { RoleId = group.Key, Permissions = group.Select(value => value.Permission).ToArray() })
            .ToDictionaryAsync(value => value.RoleId, value => (IReadOnlyList<string>)value.Permissions, cancellationToken)
            .ConfigureAwait(false);

        return roles
            .Select(role => role with
            {
                Permissions = permissions.TryGetValue(role.Id, out var rolePermissions)
                    ? rolePermissions
                    : Array.Empty<string>()
            })
            .ToArray();
    }
}
