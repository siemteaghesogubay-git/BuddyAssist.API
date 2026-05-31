using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BuddyAssist.API.Migrations
{
    /// <inheritdoc />
    public partial class AddRatingAndCompleted : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CompletedAt",
                table: "Missions",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HelperComment",
                table: "Missions",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "HelperRating",
                table: "Missions",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CompletedAt",
                table: "Missions");

            migrationBuilder.DropColumn(
                name: "HelperComment",
                table: "Missions");

            migrationBuilder.DropColumn(
                name: "HelperRating",
                table: "Missions");
        }
    }
}
