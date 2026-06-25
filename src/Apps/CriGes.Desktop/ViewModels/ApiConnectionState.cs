namespace CriGes.Desktop.ViewModels;

public enum ApiConnectionState
{
    Unknown = 0,
    ApiUnavailable = 1,
    DatabaseUnavailable = 2,
    DatabaseNotMigrated = 3,
    PlatformNotInitialized = 4,
    Ready = 5
}
