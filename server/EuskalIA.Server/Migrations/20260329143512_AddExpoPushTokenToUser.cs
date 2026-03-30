using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EuskalIA.Server.Migrations
{
    /// <inheritdoc />
    public partial class AddExpoPushTokenToUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ExpoPushToken",
                table: "Users",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ExpoPushToken",
                table: "Users");
        }
    }
}
