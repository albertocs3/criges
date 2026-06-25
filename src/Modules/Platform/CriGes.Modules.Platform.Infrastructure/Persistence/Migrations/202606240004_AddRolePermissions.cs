using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CriGes.Modules.Platform.Infrastructure.Persistence.Migrations;

public partial class AddRolePermissions : Migration
{
    private static readonly string[] AdministratorPermissions =
    [
        "Platform.ManageUsers",
        "Platform.ManageRoles",
        "Platform.ManageConfiguration",
        "Platform.ViewAudit",
        "Platform.ExportAudit",
        "Platform.ViewDiagnostics",
        "Platform.ManageBackups",
        "Platform.RestoreBackups",
        "Platform.ViewSessions",
        "Platform.CloseSessions",
        "Platform.UseAttachments"
    ];

    private static readonly string[] BillingPermissions =
    [
        "Platform.UseAttachments"
    ];

    private static readonly string[] AccountingPermissions =
    [
        "Platform.ViewAudit"
    ];

    private static readonly string[] TechnicianPermissions =
    [
        "Platform.ViewDiagnostics",
        "Platform.UseAttachments"
    ];

    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "RolePermissions",
            schema: "platform",
            columns: table => new
            {
                RoleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Permission = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                GrantedAtUtc = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: false),
                GrantedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_RolePermissions", x => new { x.RoleId, x.Permission });
                table.ForeignKey(
                    name: "FK_RolePermissions_Roles_RoleId",
                    column: x => x.RoleId,
                    principalSchema: "platform",
                    principalTable: "Roles",
                    principalColumn: "RoleId",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_RolePermissions_Permission",
            schema: "platform",
            table: "RolePermissions",
            column: "Permission");

        SeedPermissions(migrationBuilder, "ADMINISTRADOR", AdministratorPermissions);
        SeedPermissions(migrationBuilder, "FACTURACION", BillingPermissions);
        SeedPermissions(migrationBuilder, "CONTABILIDAD", AccountingPermissions);
        SeedPermissions(migrationBuilder, "TECNICO", TechnicianPermissions);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "RolePermissions", schema: "platform");
    }

    private static void SeedPermissions(MigrationBuilder migrationBuilder, string normalizedRoleName, IReadOnlyList<string> permissions)
    {
        foreach (var permission in permissions)
        {
            migrationBuilder.Sql($"""
                INSERT INTO [platform].[RolePermissions] ([RoleId], [Permission], [GrantedAtUtc], [GrantedByUserId])
                SELECT [RoleId], N'{permission}', SYSUTCDATETIME(), NULL
                FROM [platform].[Roles]
                WHERE [NormalizedName] = N'{normalizedRoleName}'
                  AND NOT EXISTS (
                      SELECT 1
                      FROM [platform].[RolePermissions]
                      WHERE [RolePermissions].[RoleId] = [Roles].[RoleId]
                        AND [RolePermissions].[Permission] = N'{permission}')
                """);
        }
    }
}
