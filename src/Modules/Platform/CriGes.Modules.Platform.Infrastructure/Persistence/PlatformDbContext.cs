using CriGes.Modules.Platform.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace CriGes.Modules.Platform.Infrastructure.Persistence;

public sealed class PlatformDbContext(DbContextOptions<PlatformDbContext> options) : DbContext(options)
{
    public DbSet<InstallationEntity> Installations => Set<InstallationEntity>();

    public DbSet<RoleEntity> Roles => Set<RoleEntity>();

    public DbSet<RolePermissionEntity> RolePermissions => Set<RolePermissionEntity>();

    public DbSet<UserEntity> Users => Set<UserEntity>();

    public DbSet<UserSessionEntity> UserSessions => Set<UserSessionEntity>();

    public DbSet<ReservedUserNameEntity> ReservedUserNames => Set<ReservedUserNameEntity>();

    public DbSet<CompanyEntity> Companies => Set<CompanyEntity>();

    public DbSet<ConfigurationVersionEntity> ConfigurationVersions => Set<ConfigurationVersionEntity>();

    public DbSet<NumberCounterEntity> NumberCounters => Set<NumberCounterEntity>();

    public DbSet<AuditEventEntity> AuditEvents => Set<AuditEventEntity>();

    public DbSet<OutboxMessageEntity> OutboxMessages => Set<OutboxMessageEntity>();

    public DbSet<HttpIdempotencyRecordEntity> HttpIdempotencyRecords => Set<HttpIdempotencyRecordEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("platform");

        ConfigureInstallations(modelBuilder);
        ConfigureRoles(modelBuilder);
        ConfigureRolePermissions(modelBuilder);
        ConfigureUsers(modelBuilder);
        ConfigureUserSessions(modelBuilder);
        ConfigureReservedUserNames(modelBuilder);
        ConfigureCompanies(modelBuilder);
        ConfigureConfigurationVersions(modelBuilder);
        ConfigureNumberCounters(modelBuilder);
        ConfigureAuditEvents(modelBuilder);
        ConfigureOutboxMessages(modelBuilder);
        ConfigureHttpIdempotencyRecords(modelBuilder);
    }

    private static void ConfigureInstallations(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<InstallationEntity>();
        entity.ToTable("Installations");
        entity.HasKey(value => value.InstallationId).HasName("PK_Installations");
        entity.Property(value => value.ProductVersion).HasMaxLength(32);
        entity.Property(value => value.FailureCode).HasMaxLength(100);
        entity.Property(value => value.StartedAtUtc).HasPrecision(3);
        entity.Property(value => value.CompletedAtUtc).HasPrecision(3);
        entity.Property(value => value.CreatedAtUtc).HasPrecision(3);
        entity.Property(value => value.RowVersion).IsRowVersion();
        entity.HasIndex(value => value.SingletonKey).IsUnique().HasDatabaseName("UX_Installations_SingletonKey");
        entity.ToTable(table =>
        {
            table.HasCheckConstraint("CK_Installations_SingletonKey", "[SingletonKey] = 1");
            table.HasCheckConstraint("CK_Installations_Status", "[Status] IN (0, 1, 2, 3)");
        });
    }

    private static void ConfigureRoles(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<RoleEntity>();
        entity.ToTable("Roles");
        entity.HasKey(value => value.RoleId).HasName("PK_Roles");
        entity.Property(value => value.Name).HasMaxLength(100);
        entity.Property(value => value.NormalizedName).HasMaxLength(100);
        entity.Property(value => value.CreatedAtUtc).HasPrecision(3);
        entity.Property(value => value.ModifiedAtUtc).HasPrecision(3);
        entity.Property(value => value.RowVersion).IsRowVersion();
        entity.HasIndex(value => value.NormalizedName).IsUnique().HasDatabaseName("UX_Roles_NormalizedName");
        entity.HasIndex(value => value.Status).HasDatabaseName("IX_Roles_Status");
        entity.ToTable(table =>
        {
            table.HasCheckConstraint("CK_Roles_RoleType", "[RoleType] IN (1, 2)");
            table.HasCheckConstraint("CK_Roles_Status", "[Status] IN (1, 2)");
            table.HasCheckConstraint("CK_Roles_ProtectedBase", "[IsProtected] = 0 OR [RoleType] = 1");
        });
    }

    private static void ConfigureUsers(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<UserEntity>();
        entity.ToTable("Users");
        entity.HasKey(value => value.UserId).HasName("PK_Users");
        entity.Property(value => value.FullName).HasMaxLength(200);
        entity.Property(value => value.UserName).HasMaxLength(100);
        entity.Property(value => value.NormalizedUserName).HasMaxLength(100);
        entity.Property(value => value.Phone).HasMaxLength(50);
        entity.Property(value => value.PasswordHash).HasMaxLength(1000);
        entity.Property(value => value.DeactivationReason).HasMaxLength(500);
        entity.Property(value => value.CreatedAtUtc).HasPrecision(3);
        entity.Property(value => value.ModifiedAtUtc).HasPrecision(3);
        entity.Property(value => value.PasswordChangedAtUtc).HasPrecision(3);
        entity.Property(value => value.BlockedUntilUtc).HasPrecision(3);
        entity.Property(value => value.LastSuccessfulLoginUtc).HasPrecision(3);
        entity.Property(value => value.DeactivatedAtUtc).HasPrecision(3);
        entity.Property(value => value.RowVersion).IsRowVersion();
        entity.HasIndex(value => value.NormalizedUserName).IsUnique().HasDatabaseName("UX_Users_NormalizedUserName");
        entity.HasIndex(value => new { value.RoleId, value.Status }).HasDatabaseName("IX_Users_RoleId_Status");
        entity.HasIndex(value => new { value.Status, value.BlockedUntilUtc }).HasDatabaseName("IX_Users_Status_BlockedUntilUtc");
        entity.HasOne<RoleEntity>().WithMany().HasForeignKey(value => value.RoleId).OnDelete(DeleteBehavior.Restrict);
        entity.ToTable(table =>
        {
            table.HasCheckConstraint("CK_Users_FailedLoginCount", "[FailedLoginCount] >= 0");
            table.HasCheckConstraint("CK_Users_Status", "[Status] IN (1, 2, 3)");
        });
    }

    private static void ConfigureRolePermissions(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<RolePermissionEntity>();
        entity.ToTable("RolePermissions");
        entity.HasKey(value => new { value.RoleId, value.Permission }).HasName("PK_RolePermissions");
        entity.Property(value => value.Permission).HasMaxLength(120);
        entity.Property(value => value.GrantedAtUtc).HasPrecision(3);
        entity.HasOne<RoleEntity>().WithMany().HasForeignKey(value => value.RoleId).OnDelete(DeleteBehavior.Cascade);
        entity.HasIndex(value => value.Permission).HasDatabaseName("IX_RolePermissions_Permission");
    }

    private static void ConfigureReservedUserNames(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<ReservedUserNameEntity>();
        entity.ToTable("ReservedUserNames");
        entity.HasKey(value => value.NormalizedUserName).HasName("PK_ReservedUserNames");
        entity.Property(value => value.NormalizedUserName).HasMaxLength(100);
        entity.Property(value => value.ReservedAtUtc).HasPrecision(3);
    }

    private static void ConfigureUserSessions(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<UserSessionEntity>();
        entity.ToTable("UserSessions");
        entity.HasKey(value => value.SessionId).HasName("PK_UserSessions");
        entity.Property(value => value.AccessToken).HasMaxLength(200);
        entity.Property(value => value.RefreshTokenHash).HasMaxLength(88);
        entity.Property(value => value.DeviceId).HasMaxLength(120);
        entity.Property(value => value.ClientVersion).HasMaxLength(32);
        entity.Property(value => value.IpAddress).HasMaxLength(64);
        entity.Property(value => value.UserAgent).HasMaxLength(512);
        entity.Property(value => value.StartedAtUtc).HasPrecision(3);
        entity.Property(value => value.LastActivityAtUtc).HasPrecision(3);
        entity.Property(value => value.IdleExpiresAtUtc).HasPrecision(3);
        entity.Property(value => value.AccessTokenExpiresAtUtc).HasPrecision(3);
        entity.Property(value => value.RefreshTokenExpiresAtUtc).HasPrecision(3);
        entity.Property(value => value.ClosedAtUtc).HasPrecision(3);
        entity.Property(value => value.RowVersion).IsRowVersion();
        entity.HasOne<UserEntity>().WithMany().HasForeignKey(value => value.UserId).OnDelete(DeleteBehavior.Restrict);
        entity.HasIndex(value => value.AccessToken).IsUnique().HasDatabaseName("UX_UserSessions_AccessToken");
        entity.HasIndex(value => value.RefreshTokenHash).IsUnique().HasDatabaseName("UX_UserSessions_RefreshTokenHash");
        entity.HasIndex(value => new { value.UserId, value.Status })
            .IsUnique()
            .HasFilter("[Status] = 1")
            .HasDatabaseName("UX_UserSessions_OneActivePerUser");
        entity.HasIndex(value => new { value.Status, value.IdleExpiresAtUtc }).HasDatabaseName("IX_UserSessions_Status_IdleExpiresAtUtc");
        entity.ToTable(table => table.HasCheckConstraint("CK_UserSessions_Status", "[Status] IN (1, 2, 3)"));
    }

    private static void ConfigureCompanies(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<CompanyEntity>();
        entity.ToTable("Companies");
        entity.HasKey(value => value.CompanyId).HasName("PK_Companies");
        entity.Property(value => value.LegalName).HasMaxLength(200);
        entity.Property(value => value.TradeName).HasMaxLength(200);
        entity.Property(value => value.TaxId).HasMaxLength(32);
        entity.Property(value => value.AddressLine).HasMaxLength(300);
        entity.Property(value => value.PostalCode).HasMaxLength(20);
        entity.Property(value => value.City).HasMaxLength(120);
        entity.Property(value => value.Region).HasMaxLength(120);
        entity.Property(value => value.CountryCode).HasMaxLength(2);
        entity.Property(value => value.Phone).HasMaxLength(50);
        entity.Property(value => value.Email).HasMaxLength(320);
        entity.Property(value => value.CreatedAtUtc).HasPrecision(3);
        entity.Property(value => value.RowVersion).IsRowVersion();
        entity.HasIndex(value => value.SingletonKey).IsUnique().HasDatabaseName("UX_Companies_SingletonKey");
        entity.ToTable(table => table.HasCheckConstraint("CK_Companies_SingletonKey", "[SingletonKey] = 1"));
    }

    private static void ConfigureConfigurationVersions(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<ConfigurationVersionEntity>();
        entity.ToTable("ConfigurationVersions");
        entity.HasKey(value => value.ConfigurationVersionId).HasName("PK_ConfigurationVersions");
        entity.Property(value => value.LanguageCode).HasMaxLength(10);
        entity.Property(value => value.CurrencyCode).HasMaxLength(3);
        entity.Property(value => value.TimeZoneId).HasMaxLength(80);
        entity.Property(value => value.ConfigurationHash).HasMaxLength(32).IsFixedLength();
        entity.Property(value => value.CreatedAtUtc).HasPrecision(3);
        entity.Property(value => value.AppliedAtUtc).HasPrecision(3);
        entity.Property(value => value.SupersededAtUtc).HasPrecision(3);
        entity.Property(value => value.RowVersion).IsRowVersion();
        entity.HasIndex(value => value.VersionNumber).IsUnique().HasDatabaseName("UX_ConfigurationVersions_VersionNumber");
        entity.HasIndex(value => value.Status)
            .IsUnique()
            .HasFilter("[Status] = 1")
            .HasDatabaseName("UX_ConfigurationVersions_Current");
    }

    private static void ConfigureNumberCounters(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<NumberCounterEntity>();
        entity.ToTable("NumberCounters");
        entity.HasKey(value => value.Code).HasName("PK_NumberCounters");
        entity.Property(value => value.Code).HasMaxLength(100);
        entity.Property(value => value.CreatedAtUtc).HasPrecision(3);
        entity.Property(value => value.RowVersion).IsRowVersion();
    }

    private static void ConfigureAuditEvents(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<AuditEventEntity>();
        entity.ToTable("AuditEvents");
        entity.HasKey(value => value.AuditEventId).HasName("PK_AuditEvents");
        entity.Property(value => value.AuditEventId).UseIdentityColumn();
        entity.Property(value => value.OccurredAtUtc).HasPrecision(3);
        entity.Property(value => value.ActorDisplayName).HasMaxLength(200);
        entity.Property(value => value.Module).HasMaxLength(80);
        entity.Property(value => value.Action).HasMaxLength(120);
        entity.Property(value => value.EntityType).HasMaxLength(120);
        entity.Property(value => value.EntityId).HasMaxLength(100);
        entity.Property(value => value.Description).HasMaxLength(2000);
        entity.Property(value => value.CreatedByNode).HasMaxLength(100);
        entity.HasIndex(value => value.OccurredAtUtc).HasDatabaseName("IX_AuditEvents_OccurredAtUtc");
        entity.HasIndex(value => new { value.ActorUserId, value.OccurredAtUtc }).HasDatabaseName("IX_AuditEvents_ActorUserId_OccurredAtUtc");
        entity.HasIndex(value => new { value.Module, value.Action, value.OccurredAtUtc }).HasDatabaseName("IX_AuditEvents_Module_Action_OccurredAtUtc");
        entity.HasIndex(value => new { value.EntityType, value.EntityId, value.OccurredAtUtc }).HasDatabaseName("IX_AuditEvents_EntityType_EntityId_OccurredAtUtc");
        entity.HasIndex(value => value.CorrelationId).HasDatabaseName("IX_AuditEvents_CorrelationId");
        entity.HasIndex(value => new { value.Result, value.OccurredAtUtc }).HasDatabaseName("IX_AuditEvents_Result_OccurredAtUtc");
    }

    private static void ConfigureOutboxMessages(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<OutboxMessageEntity>();
        entity.ToTable("OutboxMessages");
        entity.HasKey(value => value.OutboxMessageId).HasName("PK_OutboxMessages");
        entity.Property(value => value.OccurredAtUtc).HasPrecision(3);
        entity.Property(value => value.AvailableAtUtc).HasPrecision(3);
        entity.Property(value => value.NextAttemptAtUtc).HasPrecision(3);
        entity.Property(value => value.LockExpiresAtUtc).HasPrecision(3);
        entity.Property(value => value.ProcessedAtUtc).HasPrecision(3);
        entity.Property(value => value.MessageType).HasMaxLength(300);
        entity.Property(value => value.IdempotencyKey).HasMaxLength(200);
        entity.Property(value => value.LockedBy).HasMaxLength(100);
        entity.Property(value => value.LastErrorCode).HasMaxLength(100);
        entity.Property(value => value.LastErrorSafeDetail).HasMaxLength(2000);
        entity.HasIndex(value => new { value.Status, value.AvailableAtUtc }).HasDatabaseName("IX_OutboxMessages_Status_AvailableAtUtc");
        entity.HasIndex(value => new { value.NextAttemptAtUtc, value.Status }).HasDatabaseName("IX_OutboxMessages_NextAttemptAtUtc_Status");
        entity.HasIndex(value => value.CorrelationId).HasDatabaseName("IX_OutboxMessages_CorrelationId");
        entity.HasIndex(value => value.IdempotencyKey).IsUnique().HasFilter("[IdempotencyKey] IS NOT NULL").HasDatabaseName("UX_OutboxMessages_IdempotencyKey");
        entity.ToTable(table => table.HasCheckConstraint("CK_OutboxMessages_AttemptCount", "[AttemptCount] >= 0"));
    }

    private static void ConfigureHttpIdempotencyRecords(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<HttpIdempotencyRecordEntity>();
        entity.ToTable("HttpIdempotencyRecords");
        entity.HasKey(value => value.Key).HasName("PK_HttpIdempotencyRecords");
        entity.Property(value => value.Key).HasMaxLength(200);
        entity.Property(value => value.RequestHash).HasMaxLength(88);
        entity.Property(value => value.CreatedAtUtc).HasPrecision(3);
        entity.Property(value => value.CompletedAtUtc).HasPrecision(3);
        entity.HasIndex(value => value.CreatedAtUtc).HasDatabaseName("IX_HttpIdempotencyRecords_CreatedAtUtc");
    }
}
