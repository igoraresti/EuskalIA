using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EuskalIA.Server.Migrations
{
    /// <inheritdoc />
    public partial class AddSourceTrackingToExercisesV2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SourceMaterial",
                table: "AigcExercises",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SourcePage",
                table: "AigcExercises",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "BookProgresses",
                columns: table => new
                {
                    LevelId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    BookName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    LastPageProcessed = table.Column<int>(type: "int", nullable: false),
                    LastUpdated = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BookProgresses", x => x.LevelId);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BookProgresses");

            migrationBuilder.DropColumn(
                name: "SourceMaterial",
                table: "AigcExercises");

            migrationBuilder.DropColumn(
                name: "SourcePage",
                table: "AigcExercises");
        }
    }
}
