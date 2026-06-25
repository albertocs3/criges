using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CriGes.Modules.Platform.Infrastructure.Persistence.Migrations;

public partial class SeedCurrentBaseRolePermissions : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        SeedPermission(migrationBuilder, "FACTURACION", "Platform.UseAttachments");
        SeedPermission(migrationBuilder, "CONTABILIDAD", "Platform.ViewAudit");
        SeedPermission(migrationBuilder, "TECNICO", "Platform.ViewDiagnostics");
        SeedPermission(migrationBuilder, "TECNICO", "Platform.UseAttachments");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
    }

    private static void SeedPermission(MigrationBuilder migrationBuilder, string normalizedRoleName, string permission)
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
