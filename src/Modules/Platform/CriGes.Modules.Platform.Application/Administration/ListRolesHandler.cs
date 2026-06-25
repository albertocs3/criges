namespace CriGes.Modules.Platform.Application.Administration;

public sealed class ListRolesHandler(IPlatformAdministrationStore store)
{
    public Task<IReadOnlyList<RoleSummary>> HandleAsync(CancellationToken cancellationToken)
    {
        return store.ListRolesAsync(cancellationToken);
    }
}
