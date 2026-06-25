namespace CriGes.Modules.Platform.Application.Initialization;

public interface IPlatformInitializationStore
{
    Task<InstallationStatusResult> GetStatusAsync(CancellationToken cancellationToken);

    Task<bool> IsInitializedAsync(CancellationToken cancellationToken);

    Task<bool> IsUserNameReservedAsync(string normalizedUserName, CancellationToken cancellationToken);

    Task SaveInitializedPlatformAsync(PlatformInitializationSnapshot snapshot, CancellationToken cancellationToken);
}
