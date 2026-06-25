using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using CriGes.Modules.Platform.Application.Initialization;
using CriGes.Modules.Platform.Application.Auth;
using CriGes.Modules.Platform.Domain.Initialization;
using CriGes.Modules.Platform.Infrastructure.Persistence;
using CriGes.Modules.Platform.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace CriGes.Modules.Platform.Infrastructure.Initialization;

public sealed class EfPlatformInitializationStore(PlatformDbContext dbContext) : IPlatformInitializationStore
{
    public async Task<InstallationStatusResult> GetStatusAsync(CancellationToken cancellationToken)
    {
        var installation = await dbContext.Installations
            .AsNoTracking()
            .SingleOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);

        if (installation is null)
        {
            return new InstallationStatusResult("notInitialized", "1.0.0", RequiresInitialization: true);
        }

        var status = installation.Status == (byte)InstallationStatus.Initialized
            ? "initialized"
            : "notInitialized";

        return new InstallationStatusResult(status, installation.ProductVersion, installation.Status != (byte)InstallationStatus.Initialized);
    }

    public Task<bool> IsInitializedAsync(CancellationToken cancellationToken)
    {
        return dbContext.Installations.AnyAsync(
            installation => installation.Status == (byte)InstallationStatus.Initialized,
            cancellationToken);
    }

    public Task<bool> IsUserNameReservedAsync(string normalizedUserName, CancellationToken cancellationToken)
    {
        return dbContext.ReservedUserNames.AnyAsync(
            reserved => reserved.NormalizedUserName == normalizedUserName,
            cancellationToken);
    }

    public async Task SaveInitializedPlatformAsync(PlatformInitializationSnapshot snapshot, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(snapshot);

        await using var transaction = await dbContext.Database
            .BeginTransactionAsync(cancellationToken)
            .ConfigureAwait(false);

        if (await dbContext.Installations.AnyAsync(cancellationToken).ConfigureAwait(false))
        {
            throw new InvalidOperationException("The platform has already been initialized.");
        }

        var now = ToUtcDateTime(snapshot.Installation.CreatedAtUtc);
        var administratorRole = snapshot.BaseRoles.Single(role => role.NormalizedName == BaseRole.Normalize("Administrador"));

        dbContext.Installations.Add(new InstallationEntity
        {
            InstallationId = snapshot.Installation.InstallationId,
            Status = (byte)snapshot.Installation.Status,
            StartedAtUtc = ToNullableUtcDateTime(snapshot.Installation.StartedAtUtc),
            CompletedAtUtc = ToNullableUtcDateTime(snapshot.Installation.CompletedAtUtc),
            InitialAdministratorUserId = snapshot.Installation.InitialAdministratorUserId,
            ProductVersion = snapshot.Installation.ProductVersion,
            FailureCode = snapshot.Installation.FailureCode,
            CreatedAtUtc = now
        });

        foreach (var role in snapshot.BaseRoles)
        {
            dbContext.Roles.Add(new RoleEntity
            {
                RoleId = role.RoleId,
                Name = role.Name,
                NormalizedName = role.NormalizedName,
                RoleType = 1,
                Status = 1,
                IsProtected = role.IsProtected,
                PermissionVersion = 1,
                CreatedAtUtc = now,
                ModifiedAtUtc = now
            });

            foreach (var permission in RolePermissionCatalog.GetPermissionsForRole(role.Name))
            {
                dbContext.RolePermissions.Add(new RolePermissionEntity
                {
                    RoleId = role.RoleId,
                    Permission = permission,
                    GrantedAtUtc = now
                });
            }
        }

        dbContext.Users.Add(new UserEntity
        {
            UserId = snapshot.Administrator.UserId,
            FullName = snapshot.Administrator.FullName,
            UserName = snapshot.Administrator.UserName.Value,
            NormalizedUserName = snapshot.Administrator.UserName.NormalizedValue,
            RoleId = administratorRole.RoleId,
            Status = 1,
            PasswordHash = snapshot.Administrator.PasswordHash,
            SecurityVersion = 1,
            PasswordChangedAtUtc = now,
            CreatedAtUtc = now,
            ModifiedAtUtc = now
        });

        dbContext.Users.Add(new UserEntity
        {
            UserId = snapshot.SystemIdentity.UserId,
            FullName = SystemIdentity.UserName,
            UserName = SystemIdentity.UserName,
            NormalizedUserName = snapshot.SystemIdentity.NormalizedUserName,
            RoleId = administratorRole.RoleId,
            Status = 1,
            PasswordHash = "SYSTEM-IDENTITY-NO-PASSWORD",
            SecurityVersion = 1,
            PasswordChangedAtUtc = now,
            CreatedAtUtc = now,
            ModifiedAtUtc = now
        });

        dbContext.ReservedUserNames.Add(new ReservedUserNameEntity
        {
            NormalizedUserName = snapshot.Administrator.UserName.NormalizedValue,
            FirstUserId = snapshot.Administrator.UserId,
            ReservedAtUtc = now
        });
        dbContext.ReservedUserNames.Add(new ReservedUserNameEntity
        {
            NormalizedUserName = snapshot.SystemIdentity.NormalizedUserName,
            FirstUserId = snapshot.SystemIdentity.UserId,
            ReservedAtUtc = now
        });

        dbContext.Companies.Add(new CompanyEntity
        {
            CompanyId = snapshot.Company.CompanyId,
            LegalName = snapshot.Company.LegalName,
            TradeName = snapshot.Company.TradeName,
            TaxId = snapshot.Company.TaxId,
            AddressLine = snapshot.Company.Address.Line,
            PostalCode = snapshot.Company.Address.PostalCode,
            City = snapshot.Company.Address.City,
            Region = snapshot.Company.Address.Region,
            CountryCode = snapshot.Company.Address.CountryCode,
            Phone = snapshot.Company.Phone,
            Email = snapshot.Company.Email,
            CreatedAtUtc = now
        });

        dbContext.ConfigurationVersions.Add(new ConfigurationVersionEntity
        {
            ConfigurationVersionId = Guid.NewGuid(),
            VersionNumber = 1,
            Status = 1,
            LanguageCode = snapshot.RegionalConfiguration.LanguageCode,
            CurrencyCode = snapshot.RegionalConfiguration.CurrencyCode,
            TimeZoneId = snapshot.RegionalConfiguration.TimeZoneId,
            CreatedAtUtc = now,
            CreatedByUserId = snapshot.SystemIdentity.UserId,
            AppliedAtUtc = now,
            ConfigurationHash = ComputeConfigurationHash(snapshot.RegionalConfiguration)
        });

        foreach (var counter in snapshot.GlobalCounters)
        {
            dbContext.NumberCounters.Add(new NumberCounterEntity
            {
                Code = counter.Code,
                CurrentValue = counter.CurrentValue,
                CreatedAtUtc = now
            });
        }

        dbContext.AuditEvents.Add(new AuditEventEntity
        {
            OccurredAtUtc = ToUtcDateTime(snapshot.AuditEvent.OccurredAtUtc),
            ActorType = 2,
            ActorUserId = snapshot.SystemIdentity.UserId,
            ActorDisplayName = SystemIdentity.UserName,
            Module = "Platform",
            Action = snapshot.AuditEvent.EventType,
            EntityType = "Installation",
            EntityId = snapshot.Installation.InstallationId.ToString("D"),
            Result = 1,
            Description = "Platform initialized.",
            CorrelationId = Guid.NewGuid(),
            CreatedByNode = "CriGes.Api"
        });

        dbContext.OutboxMessages.Add(new OutboxMessageEntity
        {
            OutboxMessageId = Guid.NewGuid(),
            OccurredAtUtc = now,
            AvailableAtUtc = now,
            MessageType = "PlatformInitialized",
            SchemaVersion = 1,
            PayloadJson = JsonSerializer.Serialize(new { snapshot.Installation.InstallationId }),
            CorrelationId = Guid.NewGuid(),
            Status = 1,
            AttemptCount = 0
        });

        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
    }

    private static DateTime ToUtcDateTime(DateTimeOffset value)
    {
        return value.UtcDateTime;
    }

    private static DateTime? ToNullableUtcDateTime(DateTimeOffset? value)
    {
        return value?.UtcDateTime;
    }

    private static byte[] ComputeConfigurationHash(RegionalConfiguration configuration)
    {
        var value = $"{configuration.LanguageCode}|{configuration.CurrencyCode}|{configuration.TimeZoneId}";
        return SHA256.HashData(Encoding.UTF8.GetBytes(value));
    }
}
