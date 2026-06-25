namespace CriGes.Modules.Platform.Domain.Initialization;

public sealed record InitializationCompleteness(
    Guid AdministratorUserId,
    bool HasActiveAdministrator,
    bool HasBaseRoles,
    bool HasSystemIdentity,
    bool HasRegionalConfiguration,
    bool HasGlobalCounters,
    bool HasAuditEnabled);
