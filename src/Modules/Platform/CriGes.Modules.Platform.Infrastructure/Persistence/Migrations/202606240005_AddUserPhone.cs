using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CriGes.Modules.Platform.Infrastructure.Persistence.Migrations;

public partial class AddUserPhone : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "Phone",
            schema: "platform",
            table: "Users",
            type: "nvarchar(50)",
            maxLength: 50,
            nullable: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "Phone",
            schema: "platform",
            table: "Users");
    }
}
