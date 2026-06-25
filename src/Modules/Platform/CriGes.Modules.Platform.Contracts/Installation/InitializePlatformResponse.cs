namespace CriGes.Modules.Platform.Contracts.Installation;

public sealed record InitializePlatformResponse(
    Guid InstallationId,
    string Status,
    Guid AdministratorUserId,
    bool RequiresRestart);
