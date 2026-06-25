namespace CriGes.Modules.Platform.Application.Initialization;

public sealed record InitializePlatformResult(
    Guid InstallationId,
    string Status,
    Guid AdministratorUserId,
    bool RequiresRestart);
