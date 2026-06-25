using CriGes.Modules.Platform.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CriGes.Modules.Platform.Infrastructure.Persistence.Migrations;

[DbContext(typeof(PlatformDbContext))]
[Migration("202606240003_AddUserSessions")]
public partial class AddUserSessions : Migration
{
    private static readonly string[] StatusIdleExpiresAtUtcColumns = ["Status", "IdleExpiresAtUtc"];
    private static readonly string[] UserIdStatusColumns = ["UserId", "Status"];

    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "UserSessions",
            schema: "platform",
            columns: table => new
            {
                SessionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Status = table.Column<byte>(type: "tinyint", nullable: false),
                AccessToken = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                AccessTokenExpiresAtUtc = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: false),
                RefreshTokenHash = table.Column<string>(type: "nvarchar(88)", maxLength: 88, nullable: false),
                RefreshTokenExpiresAtUtc = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: false),
                StartedAtUtc = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: false),
                LastActivityAtUtc = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: false),
                IdleExpiresAtUtc = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: false),
                ClosedAtUtc = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: true),
                DeviceId = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                ClientVersion = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                IpAddress = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                UserAgent = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true),
                SecurityVersion = table.Column<long>(type: "bigint", nullable: false),
                RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_UserSessions", x => x.SessionId);
                table.CheckConstraint("CK_UserSessions_Status", "[Status] IN (1, 2, 3)");
                table.ForeignKey(
                    name: "FK_UserSessions_Users_UserId",
                    column: x => x.UserId,
                    principalSchema: "platform",
                    principalTable: "Users",
                    principalColumn: "UserId",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateIndex(
            name: "IX_UserSessions_Status_IdleExpiresAtUtc",
            schema: "platform",
            table: "UserSessions",
            columns: StatusIdleExpiresAtUtcColumns);

        migrationBuilder.CreateIndex(
            name: "UX_UserSessions_AccessToken",
            schema: "platform",
            table: "UserSessions",
            column: "AccessToken",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "UX_UserSessions_OneActivePerUser",
            schema: "platform",
            table: "UserSessions",
            columns: UserIdStatusColumns,
            unique: true,
            filter: "[Status] = 1");

        migrationBuilder.CreateIndex(
            name: "UX_UserSessions_RefreshTokenHash",
            schema: "platform",
            table: "UserSessions",
            column: "RefreshTokenHash",
            unique: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "UserSessions", schema: "platform");
    }
}
