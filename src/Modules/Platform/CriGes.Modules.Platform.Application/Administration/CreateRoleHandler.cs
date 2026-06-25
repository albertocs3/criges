using CriGes.Application.Abstractions;
using CriGes.SharedKernel;

namespace CriGes.Modules.Platform.Application.Administration;

public sealed class CreateRoleHandler(
    IPlatformAdministrationStore store,
    IIdGenerator idGenerator,
    IClock clock,
    ICorrelationContext correlationContext)
{
    public async Task<Result<RoleSummary>> HandleAsync(
        CreateRoleCommand command,
        Guid? actorUserId,
        CancellationToken cancellationToken)
    {
        var name = command.Name?.Trim();
        if (string.IsNullOrWhiteSpace(name) || name.Length is < 3 or > 100)
        {
            return Result.Failure<RoleSummary>(AdministrationErrors.ValidationFailed);
        }

        var normalizedName = name.ToUpperInvariant();
        if (await store.IsRoleNameReservedAsync(normalizedName, cancellationToken).ConfigureAwait(false))
        {
            return Result.Failure<RoleSummary>(AdministrationErrors.RoleNameAlreadyReserved);
        }

        var role = new RoleCreationData(
            idGenerator.NewId(),
            name,
            normalizedName,
            clock.UtcNow);

        var created = await store.CreateRoleAsync(
                role,
                actorUserId,
                correlationContext.CorrelationId,
                cancellationToken)
            .ConfigureAwait(false);

        return Result.Success(created);
    }
}
