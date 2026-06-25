namespace CriGes.Modules.Platform.Domain.Initialization;

public enum InstallationStatus : byte
{
    NotInitialized = 0,
    Initializing = 1,
    Initialized = 2,
    Failed = 3
}
