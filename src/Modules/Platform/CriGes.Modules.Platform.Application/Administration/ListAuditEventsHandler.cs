namespace CriGes.Modules.Platform.Application.Administration;

public sealed class ListAuditEventsHandler(IPlatformAdministrationStore store)
{
    public Task<IReadOnlyList<AuditEventSummary>> HandleAsync(
        int take,
        CancellationToken cancellationToken)
    {
        var boundedTake = Math.Clamp(take, 1, 100);
        return store.ListAuditEventsAsync(boundedTake, cancellationToken);
    }
}
