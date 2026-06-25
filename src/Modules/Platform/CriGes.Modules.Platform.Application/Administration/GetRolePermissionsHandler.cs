using CriGes.SharedKernel;

namespace CriGes.Modules.Platform.Application.Administration;

public sealed class GetRolePermissionsHandler(IPlatformAdministrationStore store)
{
    public async Task<Result<IReadOnlyList<string>>> HandleAsync(Guid roleId, CancellationToken cancellationToken)
    {
        var role = await store.FindRoleAsync(roleId, cancellationToken).ConfigureAwait(false);
        if (role is null)
        {
            return Result.Failure<IReadOnlyList<string>>(AdministrationErrors.RoleNotFound);
        }

        return Result.Success(role.Permissions);
    }
}
