namespace CriGes.Modules.Platform.Application.Initialization;

public sealed class GetInstallationStatusHandler(IPlatformInitializationStore store)
{
    public Task<InstallationStatusResult> HandleAsync(CancellationToken cancellationToken)
    {
        return store.GetStatusAsync(cancellationToken);
    }
}
