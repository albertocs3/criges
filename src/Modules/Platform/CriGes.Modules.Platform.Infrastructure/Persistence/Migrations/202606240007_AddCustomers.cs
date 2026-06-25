using CriGes.Modules.Platform.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CriGes.Modules.Platform.Infrastructure.Persistence.Migrations;

[DbContext(typeof(PlatformDbContext))]
[Migration("202606240007_AddCustomers")]
public partial class AddCustomers : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.EnsureSchema(name: "operations");

        migrationBuilder.CreateTable(
            name: "Customers",
            schema: "operations",
            columns: table => new
            {
                CustomerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                NormalizedName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                TaxId = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: true),
                NormalizedTaxId = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: true),
                Email = table.Column<string>(type: "nvarchar(320)", maxLength: 320, nullable: true),
                Phone = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                Status = table.Column<byte>(type: "tinyint", nullable: false),
                CreatedAtUtc = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: false),
                ModifiedAtUtc = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: false),
                CreatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                ModifiedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Customers", x => x.CustomerId);
                table.CheckConstraint("CK_Customers_Status", "[Status] IN (1, 2)");
            });

        migrationBuilder.CreateIndex(
            name: "IX_Customers_NormalizedName",
            schema: "operations",
            table: "Customers",
            column: "NormalizedName");

        migrationBuilder.CreateIndex(
            name: "IX_Customers_Status",
            schema: "operations",
            table: "Customers",
            column: "Status");

        migrationBuilder.CreateIndex(
            name: "UX_Customers_NormalizedTaxId",
            schema: "operations",
            table: "Customers",
            column: "NormalizedTaxId",
            unique: true,
            filter: "[NormalizedTaxId] IS NOT NULL");

        SeedPermission(migrationBuilder, "ADMINISTRADOR", "Customers.View");
        SeedPermission(migrationBuilder, "ADMINISTRADOR", "Customers.Manage");
        SeedPermission(migrationBuilder, "FACTURACION", "Customers.View");
        SeedPermission(migrationBuilder, "FACTURACION", "Customers.Manage");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "Customers",
            schema: "operations");
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
