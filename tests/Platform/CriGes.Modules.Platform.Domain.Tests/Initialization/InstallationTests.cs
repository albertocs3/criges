using CriGes.Modules.Platform.Domain.Initialization;

namespace CriGes.Modules.Platform.Domain.Tests.Initialization;

public sealed class InstallationTests
{
    [Fact]
    public void CompleteInitializationFailsWhenRequiredPiecesAreMissing()
    {
        var now = DateTimeOffset.Parse("2026-01-01T00:00:00Z", System.Globalization.CultureInfo.InvariantCulture);
        var installation = Installation.CreateNew(Guid.NewGuid(), "1.0.0", now);

        installation.StartInitialization(now);

        var result = installation.CompleteInitialization(
            new InitializationCompleteness(
                Guid.NewGuid(),
                HasActiveAdministrator: true,
                HasBaseRoles: false,
                HasSystemIdentity: true,
                HasRegionalConfiguration: true,
                HasGlobalCounters: true,
                HasAuditEnabled: true),
            now);

        Assert.True(result.IsFailure);
        Assert.Equal("PLATFORM.INITIALIZATION_FAILED", result.Error.Code);
    }

    [Fact]
    public void CompleteInitializationMarksInstallationInitializedWhenAllPiecesExist()
    {
        var now = DateTimeOffset.Parse("2026-01-01T00:00:00Z", System.Globalization.CultureInfo.InvariantCulture);
        var administratorUserId = Guid.NewGuid();
        var installation = Installation.CreateNew(Guid.NewGuid(), "1.0.0", now);

        installation.StartInitialization(now);

        var result = installation.CompleteInitialization(
            new InitializationCompleteness(
                administratorUserId,
                HasActiveAdministrator: true,
                HasBaseRoles: true,
                HasSystemIdentity: true,
                HasRegionalConfiguration: true,
                HasGlobalCounters: true,
                HasAuditEnabled: true),
            now);

        Assert.True(result.IsSuccess);
        Assert.Equal(InstallationStatus.Initialized, installation.Status);
        Assert.Equal(administratorUserId, installation.InitialAdministratorUserId);
    }
}
