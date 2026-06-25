using CriGes.Application.Abstractions;
using CriGes.Modules.Platform.Domain.Initialization;
using CriGes.SharedKernel;

namespace CriGes.Modules.Platform.Application.Initialization;

public sealed class InitializePlatformHandler(
    IPlatformInitializationStore store,
    IClock clock,
    IIdGenerator idGenerator,
    IPasswordHasher passwordHasher)
{
    private const string ProductVersion = "1.0.0";

    public async Task<Result<InitializePlatformResult>> HandleAsync(
        InitializePlatformCommand command,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        if (await store.IsInitializedAsync(cancellationToken).ConfigureAwait(false))
        {
            return Result.Failure<InitializePlatformResult>(PlatformErrors.AlreadyInitialized);
        }

        var userName = UserName.Create(command.Administrator.UserName);
        if (userName.IsFailure)
        {
            return Result.Failure<InitializePlatformResult>(userName.Error);
        }

        if (await store.IsUserNameReservedAsync(userName.Value.NormalizedValue, cancellationToken).ConfigureAwait(false))
        {
            return Result.Failure<InitializePlatformResult>(PlatformErrors.UserNameAlreadyReserved);
        }

        var passwordPolicy = PasswordPolicy.ValidateInitialPassword(command.Administrator.Password);
        if (passwordPolicy.IsFailure)
        {
            return Result.Failure<InitializePlatformResult>(passwordPolicy.Error);
        }

        var address = CompanyAddress.Create(
            command.Company.Address.Line,
            command.Company.Address.PostalCode,
            command.Company.Address.City,
            command.Company.Address.Region,
            command.Company.Address.CountryCode);
        if (address.IsFailure)
        {
            return Result.Failure<InitializePlatformResult>(address.Error);
        }

        var company = Company.Create(
            idGenerator.NewId(),
            command.Company.LegalName,
            command.Company.TradeName,
            command.Company.TaxId,
            address.Value,
            command.Company.Phone,
            command.Company.Email);
        if (company.IsFailure)
        {
            return Result.Failure<InitializePlatformResult>(company.Error);
        }

        if (string.IsNullOrWhiteSpace(command.Administrator.FullName))
        {
            return Result.Failure<InitializePlatformResult>(PlatformErrors.ValidationFailed);
        }

        var now = clock.UtcNow;
        var installation = Installation.CreateNew(idGenerator.NewId(), ProductVersion, now);
        var start = installation.StartInitialization(now);
        if (start.IsFailure)
        {
            return Result.Failure<InitializePlatformResult>(start.Error);
        }

        var baseRoles = BaseRole.RequiredNames.Select(roleName => BaseRole.Create(idGenerator.NewId(), roleName)).ToArray();
        var administratorRole = baseRoles.Single(role => role.NormalizedName == BaseRole.Normalize("Administrador"));
        var administrator = new InitialAdministrator(
            idGenerator.NewId(),
            command.Administrator.FullName.Trim(),
            userName.Value,
            passwordHasher.HashPassword(command.Administrator.Password!),
            administratorRole.RoleId,
            now);
        var systemIdentity = SystemIdentity.Create(idGenerator.NewId(), now);
        var regionalConfiguration = RegionalConfiguration.InitialSpain();
        var counters = NumberCounter.InitialCounters;
        var auditEvent = AuditEvent.PlatformInitialized(idGenerator.NewId(), now);

        var completeness = new InitializationCompleteness(
            administrator.UserId,
            administrator.UserId != Guid.Empty,
            baseRoles.Length == BaseRole.RequiredNames.Count && baseRoles.All(role => role.IsProtected),
            !string.IsNullOrWhiteSpace(systemIdentity.NormalizedUserName),
            regionalConfiguration == RegionalConfiguration.InitialSpain(),
            counters.Count > 0,
            auditEvent.EventType == "PlatformInitialized");

        var complete = installation.CompleteInitialization(completeness, now);
        if (complete.IsFailure)
        {
            installation.MarkFailed(complete.Error);
            return Result.Failure<InitializePlatformResult>(complete.Error);
        }

        var snapshot = new PlatformInitializationSnapshot(
            installation,
            baseRoles,
            administrator,
            systemIdentity,
            company.Value,
            regionalConfiguration,
            counters,
            auditEvent);

        await store.SaveInitializedPlatformAsync(snapshot, cancellationToken).ConfigureAwait(false);

        return Result.Success(new InitializePlatformResult(
            installation.InstallationId,
            "initialized",
            administrator.UserId,
            RequiresRestart: false));
    }
}
