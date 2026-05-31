using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BuddyAssist.API.Migrations
{
    /// <inheritdoc />
    public partial class AddSponsoredMissions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsSponsored",
                table: "Missions",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "SponsorBudget",
                table: "Missions",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SponsorClicks",
                table: "Missions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "SponsorExpiresAt",
                table: "Missions",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SponsorLogo",
                table: "Missions",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SponsorName",
                table: "Missions",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SponsorUrl",
                table: "Missions",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsSponsored",
                table: "Missions");

            migrationBuilder.DropColumn(
                name: "SponsorBudget",
                table: "Missions");

            migrationBuilder.DropColumn(
                name: "SponsorClicks",
                table: "Missions");

            migrationBuilder.DropColumn(
                name: "SponsorExpiresAt",
                table: "Missions");

            migrationBuilder.DropColumn(
                name: "SponsorLogo",
                table: "Missions");

            migrationBuilder.DropColumn(
                name: "SponsorName",
                table: "Missions");

            migrationBuilder.DropColumn(
                name: "SponsorUrl",
                table: "Missions");
        }
    }
}
