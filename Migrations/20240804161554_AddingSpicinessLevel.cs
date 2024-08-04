using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Feast.Migrations
{
    /// <inheritdoc />
    public partial class AddingSpicinessLevel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SpicinessLevel",
                table: "Recipes",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SpicinessLevel",
                table: "Recipes");
        }
    }
}
