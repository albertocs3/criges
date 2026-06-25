using System;
using CriGes.Modules.Platform.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CriGes.Modules.Platform.Infrastructure.Persistence.Migrations;

[DbContext(typeof(PlatformDbContext))]
[Migration("202606240002_AddHttpIdempotencyRecords")]
public partial class AddHttpIdempotencyRecords : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "HttpIdempotencyRecords",
            schema: "platform",
            columns: table => new
            {
                Key = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                RequestHash = table.Column<string>(type: "nvarchar(88)", maxLength: 88, nullable: false),
                StatusCode = table.Column<int>(type: "int", nullable: true),
                ResponseJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                CreatedAtUtc = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: false),
                CompletedAtUtc = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: true)
            },
            constraints: table => table.PrimaryKey("PK_HttpIdempotencyRecords", x => x.Key));

        migrationBuilder.CreateIndex(
            name: "IX_HttpIdempotencyRecords_CreatedAtUtc",
            schema: "platform",
            table: "HttpIdempotencyRecords",
            column: "CreatedAtUtc");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "HttpIdempotencyRecords", schema: "platform");
    }
}
