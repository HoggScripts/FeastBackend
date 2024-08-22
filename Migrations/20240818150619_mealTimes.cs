using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Feast.Migrations
{
    /// <inheritdoc />
    public partial class mealTimes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<TimeSpan>(
                name: "BreakfastTime",
                table: "AspNetUsers",
                type: "interval",
                nullable: false,
                defaultValue: new TimeSpan(0, 0, 0, 0, 0));

            migrationBuilder.AddColumn<TimeSpan>(
                name: "DinnerTime",
                table: "AspNetUsers",
                type: "interval",
                nullable: false,
                defaultValue: new TimeSpan(0, 0, 0, 0, 0));

            migrationBuilder.AddColumn<TimeSpan>(
                name: "LunchTime",
                table: "AspNetUsers",
                type: "interval",
                nullable: false,
                defaultValue: new TimeSpan(0, 0, 0, 0, 0));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BreakfastTime",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "DinnerTime",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "LunchTime",
                table: "AspNetUsers");
        }
    }
}
