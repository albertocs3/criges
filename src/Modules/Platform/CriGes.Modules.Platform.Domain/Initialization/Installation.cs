using CriGes.SharedKernel;

namespace CriGes.Modules.Platform.Domain.Initialization;

public sealed class Installation
{
    private Installation(Guid installationId, string productVersion, DateTimeOffset createdAtUtc)
    {
        InstallationId = installationId;
        ProductVersion = productVersion;
        CreatedAtUtc = createdAtUtc;
        Status = InstallationStatus.NotInitialized;
    }

    public Guid InstallationId { get; }

    public InstallationStatus Status { get; private set; }

    public DateTimeOffset? StartedAtUtc { get; private set; }

    public DateTimeOffset? CompletedAtUtc { get; private set; }

    public Guid? InitialAdministratorUserId { get; private set; }

    public string ProductVersion { get; }

    public string? FailureCode { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; }

    public static Installation CreateNew(Guid installationId, string productVersion, DateTimeOffset createdAtUtc)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(productVersion);
        return new Installation(installationId, productVersion, createdAtUtc);
    }

    public Result StartInitialization(DateTimeOffset startedAtUtc)
    {
        if (Status is InstallationStatus.Initialized or InstallationStatus.Initializing)
        {
            return Result.Failure(PlatformErrors.AlreadyInitialized);
        }

        Status = InstallationStatus.Initializing;
        StartedAtUtc = startedAtUtc;
        FailureCode = null;

        return Result.Success();
    }

    public Result CompleteInitialization(InitializationCompleteness completeness, DateTimeOffset completedAtUtc)
    {
        if (Status != InstallationStatus.Initializing)
        {
            return Result.Failure(PlatformErrors.InitializationFailed);
        }

        if (!completeness.HasActiveAdministrator ||
            !completeness.HasBaseRoles ||
            !completeness.HasSystemIdentity ||
            !completeness.HasRegionalConfiguration ||
            !completeness.HasGlobalCounters ||
            !completeness.HasAuditEnabled)
        {
            return Result.Failure(PlatformErrors.InitializationFailed);
        }

        Status = InstallationStatus.Initialized;
        CompletedAtUtc = completedAtUtc;
        InitialAdministratorUserId = completeness.AdministratorUserId;

        return Result.Success();
    }

    public void MarkFailed(AppError error)
    {
        Status = InstallationStatus.Failed;
        FailureCode = error.Code;
    }
}
