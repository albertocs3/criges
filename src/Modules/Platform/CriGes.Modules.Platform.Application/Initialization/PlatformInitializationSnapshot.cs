using CriGes.Modules.Platform.Domain.Initialization;

namespace CriGes.Modules.Platform.Application.Initialization;

public sealed record PlatformInitializationSnapshot(
    Installation Installation,
    IReadOnlyList<BaseRole> BaseRoles,
    InitialAdministrator Administrator,
    SystemIdentity SystemIdentity,
    Company Company,
    RegionalConfiguration RegionalConfiguration,
    IReadOnlyList<NumberCounter> GlobalCounters,
    AuditEvent AuditEvent);
