namespace CriGes.Modules.Platform.Application.Administration;

public sealed class ListUsersHandler(IPlatformAdministrationStore store)
{
    public Task<IReadOnlyList<UserSummary>> HandleAsync(CancellationToken cancellationToken)
    {
        return store.ListUsersAsync(cancellationToken);
    }
}
