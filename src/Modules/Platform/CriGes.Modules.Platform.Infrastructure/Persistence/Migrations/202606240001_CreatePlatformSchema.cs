using System;
using CriGes.Modules.Platform.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable
#pragma warning disable CA1861

namespace CriGes.Modules.Platform.Infrastructure.Persistence.Migrations;

[DbContext(typeof(PlatformDbContext))]
[Migration("202606240001_CreatePlatformSchema")]
public partial class CreatePlatformSchema : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.EnsureSchema(name: "platform");

        migrationBuilder.CreateTable(
            name: "Roles",
            schema: "platform",
            columns: table => new
            {
                RoleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                NormalizedName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                RoleType = table.Column<byte>(type: "tinyint", nullable: false),
                Status = table.Column<byte>(type: "tinyint", nullable: false),
                IsProtected = table.Column<bool>(type: "bit", nullable: false),
                PermissionVersion = table.Column<long>(type: "bigint", nullable: false),
                CreatedAtUtc = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: false),
                ModifiedAtUtc = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: false),
                CreatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                ModifiedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Roles", x => x.RoleId);
                table.CheckConstraint("CK_Roles_ProtectedBase", "[IsProtected] = 0 OR [RoleType] = 1");
                table.CheckConstraint("CK_Roles_RoleType", "[RoleType] IN (1, 2)");
                table.CheckConstraint("CK_Roles_Status", "[Status] IN (1, 2)");
            });

        migrationBuilder.CreateTable(
            name: "Users",
            schema: "platform",
            columns: table => new
            {
                UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                FullName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                UserName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                NormalizedUserName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                RoleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Status = table.Column<byte>(type: "tinyint", nullable: false),
                PasswordHash = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                SecurityVersion = table.Column<long>(type: "bigint", nullable: false),
                FailedLoginCount = table.Column<short>(type: "smallint", nullable: false),
                BlockedUntilUtc = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: true),
                LastSuccessfulLoginUtc = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: true),
                PasswordChangedAtUtc = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: false),
                CreatedAtUtc = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: false),
                DeactivatedAtUtc = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: true),
                DeactivationReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                CreatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                ModifiedAtUtc = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: false),
                ModifiedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Users", x => x.UserId);
                table.ForeignKey(
                    name: "FK_Users_Roles_RoleId",
                    column: x => x.RoleId,
                    principalSchema: "platform",
                    principalTable: "Roles",
                    principalColumn: "RoleId",
                    onDelete: ReferentialAction.Restrict);
                table.CheckConstraint("CK_Users_FailedLoginCount", "[FailedLoginCount] >= 0");
                table.CheckConstraint("CK_Users_Status", "[Status] IN (1, 2, 3)");
            });

        migrationBuilder.CreateTable(
            name: "Installations",
            schema: "platform",
            columns: table => new
            {
                InstallationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                SingletonKey = table.Column<byte>(type: "tinyint", nullable: false),
                Status = table.Column<byte>(type: "tinyint", nullable: false),
                StartedAtUtc = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: true),
                CompletedAtUtc = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: true),
                InitialAdministratorUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                ProductVersion = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                FailureCode = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                CreatedAtUtc = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: false),
                RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Installations", x => x.InstallationId);
                table.CheckConstraint("CK_Installations_SingletonKey", "[SingletonKey] = 1");
                table.CheckConstraint("CK_Installations_Status", "[Status] IN (0, 1, 2, 3)");
            });

        migrationBuilder.CreateTable(
            name: "ReservedUserNames",
            schema: "platform",
            columns: table => new
            {
                NormalizedUserName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                FirstUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                ReservedAtUtc = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: false)
            },
            constraints: table => table.PrimaryKey("PK_ReservedUserNames", x => x.NormalizedUserName));

        migrationBuilder.CreateTable(
            name: "Companies",
            schema: "platform",
            columns: table => new
            {
                CompanyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                SingletonKey = table.Column<byte>(type: "tinyint", nullable: false),
                LegalName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                TradeName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                TaxId = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                AddressLine = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                PostalCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                City = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                Region = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                CountryCode = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: false),
                Phone = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                Email = table.Column<string>(type: "nvarchar(320)", maxLength: 320, nullable: true),
                CreatedAtUtc = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: false),
                RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Companies", x => x.CompanyId);
                table.CheckConstraint("CK_Companies_SingletonKey", "[SingletonKey] = 1");
            });

        migrationBuilder.CreateTable(
            name: "ConfigurationVersions",
            schema: "platform",
            columns: table => new
            {
                ConfigurationVersionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                VersionNumber = table.Column<long>(type: "bigint", nullable: false),
                Status = table.Column<byte>(type: "tinyint", nullable: false),
                LanguageCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                CurrencyCode = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                TimeZoneId = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                CreatedAtUtc = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: false),
                CreatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                AppliedAtUtc = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: true),
                SupersededAtUtc = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: true),
                ConfigurationHash = table.Column<byte[]>(type: "binary(32)", fixedLength: true, maxLength: 32, nullable: false),
                RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
            },
            constraints: table => table.PrimaryKey("PK_ConfigurationVersions", x => x.ConfigurationVersionId));

        migrationBuilder.CreateTable(
            name: "NumberCounters",
            schema: "platform",
            columns: table => new
            {
                Code = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                CurrentValue = table.Column<long>(type: "bigint", nullable: false),
                CreatedAtUtc = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: false),
                RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
            },
            constraints: table => table.PrimaryKey("PK_NumberCounters", x => x.Code));

        migrationBuilder.CreateTable(
            name: "AuditEvents",
            schema: "platform",
            columns: table => new
            {
                AuditEventId = table.Column<long>(type: "bigint", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                OccurredAtUtc = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: false),
                ActorType = table.Column<byte>(type: "tinyint", nullable: false),
                ActorUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                ActorDisplayName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                Module = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                Action = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                EntityType = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                EntityId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                Result = table.Column<byte>(type: "tinyint", nullable: false),
                PreviousValuesJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                NewValuesJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                CorrelationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                CreatedByNode = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
            },
            constraints: table => table.PrimaryKey("PK_AuditEvents", x => x.AuditEventId));

        migrationBuilder.CreateTable(
            name: "OutboxMessages",
            schema: "platform",
            columns: table => new
            {
                OutboxMessageId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                OccurredAtUtc = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: false),
                AvailableAtUtc = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: false),
                MessageType = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                SchemaVersion = table.Column<int>(type: "int", nullable: false),
                PayloadJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                HeadersJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                CorrelationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                IdempotencyKey = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                Status = table.Column<byte>(type: "tinyint", nullable: false),
                AttemptCount = table.Column<int>(type: "int", nullable: false),
                NextAttemptAtUtc = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: true),
                LockedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                LockExpiresAtUtc = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: true),
                ProcessedAtUtc = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: true),
                LastErrorCode = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                LastErrorSafeDetail = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_OutboxMessages", x => x.OutboxMessageId);
                table.CheckConstraint("CK_OutboxMessages_AttemptCount", "[AttemptCount] >= 0");
            });

        migrationBuilder.CreateIndex("UX_Roles_NormalizedName", "Roles", "NormalizedName", "platform", unique: true);
        migrationBuilder.CreateIndex("IX_Roles_Status", "Roles", "Status", "platform");
        migrationBuilder.CreateIndex("UX_Users_NormalizedUserName", "Users", "NormalizedUserName", "platform", unique: true);
        migrationBuilder.CreateIndex("IX_Users_RoleId_Status", "Users", new[] { "RoleId", "Status" }, "platform");
        migrationBuilder.CreateIndex("IX_Users_Status_BlockedUntilUtc", "Users", new[] { "Status", "BlockedUntilUtc" }, "platform");
        migrationBuilder.CreateIndex("UX_Installations_SingletonKey", "Installations", "SingletonKey", "platform", unique: true);
        migrationBuilder.CreateIndex("UX_Companies_SingletonKey", "Companies", "SingletonKey", "platform", unique: true);
        migrationBuilder.CreateIndex("UX_ConfigurationVersions_VersionNumber", "ConfigurationVersions", "VersionNumber", "platform", unique: true);
        migrationBuilder.CreateIndex("UX_ConfigurationVersions_Current", "ConfigurationVersions", "Status", "platform", unique: true, filter: "[Status] = 1");
        migrationBuilder.CreateIndex("IX_AuditEvents_OccurredAtUtc", "AuditEvents", "OccurredAtUtc", "platform");
        migrationBuilder.CreateIndex("IX_AuditEvents_ActorUserId_OccurredAtUtc", "AuditEvents", new[] { "ActorUserId", "OccurredAtUtc" }, "platform");
        migrationBuilder.CreateIndex("IX_AuditEvents_Module_Action_OccurredAtUtc", "AuditEvents", new[] { "Module", "Action", "OccurredAtUtc" }, "platform");
        migrationBuilder.CreateIndex("IX_AuditEvents_EntityType_EntityId_OccurredAtUtc", "AuditEvents", new[] { "EntityType", "EntityId", "OccurredAtUtc" }, "platform");
        migrationBuilder.CreateIndex("IX_AuditEvents_CorrelationId", "AuditEvents", "CorrelationId", "platform");
        migrationBuilder.CreateIndex("IX_AuditEvents_Result_OccurredAtUtc", "AuditEvents", new[] { "Result", "OccurredAtUtc" }, "platform");
        migrationBuilder.CreateIndex("IX_OutboxMessages_Status_AvailableAtUtc", "OutboxMessages", new[] { "Status", "AvailableAtUtc" }, "platform");
        migrationBuilder.CreateIndex("IX_OutboxMessages_NextAttemptAtUtc_Status", "OutboxMessages", new[] { "NextAttemptAtUtc", "Status" }, "platform");
        migrationBuilder.CreateIndex("IX_OutboxMessages_CorrelationId", "OutboxMessages", "CorrelationId", "platform");
        migrationBuilder.CreateIndex("UX_OutboxMessages_IdempotencyKey", "OutboxMessages", "IdempotencyKey", "platform", unique: true, filter: "[IdempotencyKey] IS NOT NULL");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "OutboxMessages", schema: "platform");
        migrationBuilder.DropTable(name: "AuditEvents", schema: "platform");
        migrationBuilder.DropTable(name: "NumberCounters", schema: "platform");
        migrationBuilder.DropTable(name: "ConfigurationVersions", schema: "platform");
        migrationBuilder.DropTable(name: "Companies", schema: "platform");
        migrationBuilder.DropTable(name: "ReservedUserNames", schema: "platform");
        migrationBuilder.DropTable(name: "Installations", schema: "platform");
        migrationBuilder.DropTable(name: "Users", schema: "platform");
        migrationBuilder.DropTable(name: "Roles", schema: "platform");
    }
}
