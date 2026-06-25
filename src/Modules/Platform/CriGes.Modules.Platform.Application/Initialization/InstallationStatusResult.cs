namespace CriGes.Modules.Platform.Application.Initialization;

public sealed record InstallationStatusResult(
    string Status,
    string ProductVersion,
    bool RequiresInitialization);
