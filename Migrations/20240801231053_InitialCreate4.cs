﻿using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Feast.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate4 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Ingredient",
                newName: "Id1");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Id1",
                table: "Ingredient",
                newName: "Id");
        }
    }
}
