using CriGes.Application.Abstractions;
using CriGes.Modules.Platform.Contracts.Auth;
using CriGes.SharedKernel;

namespace CriGes.Modules.Platform.Application.Administration;

public sealed class UpdateRolePermissionsHandler(
    IPlatformAdministrationStore store,
    IClock clock,
    ICorrelationContext correlationContext)
{
    public async Task<Result<IReadOnlyList<string>>> HandleAsync(
        UpdateRolePermissionsCommand command,
        Guid? actorUserId,
        CancellationToken cancellationToken)
    {
        if (command.RoleId == Guid.Empty || command.Permissions is null)
        {
            return Result.Failure<IReadOnlyList<string>>(AdministrationErrors.ValidationFailed);
        }

        var role = await store.FindRoleAsync(command.RoleId, cancellationToken).ConfigureAwait(false);
        if (role is null)
        {
            return Result.Failure<IReadOnlyList<string>>(AdministrationErrors.RoleNotFound);
        }

        if (role.Status != 1)
        {
            return Result.Failure<IReadOnlyList<string>>(AdministrationErrors.RoleInactive);
        }

        var normalized = command.Permissions
            .Where(permission => !string.IsNullOrWhiteSpace(permission))
            .Select(permission => permission.Trim())
            .Distinct(StringComparer.Ordinal)
            .OrderBy(permission => permission, StringComparer.Ordinal)
            .ToArray();

        if (normalized.Length != command.Permissions.Count ||
            normalized.Any(permission => !PlatformPermissionNames.All.Contains(permission)))
        {
            return Result.Failure<IReadOnlyList<string>>(AdministrationErrors.UnknownPermission);
        }

        var permissions = await store.ReplaceRolePermissionsAsync(
                role.Id,
                normalized,
                actorUserId,
                clock.UtcNow,
                correlationContext.CorrelationId,
                cancellationToken)
            .ConfigureAwait(false);

        return Result.Success(permissions);
    }
}
