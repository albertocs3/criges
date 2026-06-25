namespace CriGes.Modules.Platform.Contracts.Installation;

public sealed record InstallationStatusResponse(
    string Status,
    string ProductVersion,
    bool RequiresInitialization);
