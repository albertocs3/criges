using CriGes.Modules.Platform.Application.Initialization;

namespace CriGes.Modules.Platform.Infrastructure.Initialization;

public sealed class InMemoryPlatformInitializationStore : IPlatformInitializationStore
{
    private readonly object syncRoot = new();
    private PlatformInitializationSnapshot? snapshot;
    private readonly HashSet<string> reservedUserNames = new(StringComparer.Ordinal);

    public Task<InstallationStatusResult> GetStatusAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        lock (syncRoot)
        {
            if (snapshot is null)
            {
                return Task.FromResult(new InstallationStatusResult("notInitialized", "1.0.0", RequiresInitialization: true));
            }

            return Task.FromResult(new InstallationStatusResult("initialized", snapshot.Installation.ProductVersion, RequiresInitialization: false));
        }
    }

    public Task<bool> IsInitializedAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        lock (syncRoot)
        {
            return Task.FromResult(snapshot is not null);
        }
    }

    public Task<bool> IsUserNameReservedAsync(string normalizedUserName, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        lock (syncRoot)
        {
            return Task.FromResult(reservedUserNames.Contains(normalizedUserName));
        }
    }

    public Task SaveInitializedPlatformAsync(PlatformInitializationSnapshot snapshot, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        cancellationToken.ThrowIfCancellationRequested();

        lock (syncRoot)
        {
            if (this.snapshot is not null)
            {
                throw new InvalidOperationException("The platform has already been initialized.");
            }

            this.snapshot = snapshot;
            reservedUserNames.Add(snapshot.Administrator.UserName.NormalizedValue);
            reservedUserNames.Add(snapshot.SystemIdentity.NormalizedUserName);
        }

        return Task.CompletedTask;
    }

    public PlatformInitializationSnapshot? GetSnapshot()
    {
        lock (syncRoot)
        {
            return snapshot;
        }
    }
}
