using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BuddyAssist.API.Migrations
{
    /// <inheritdoc />
    public partial class AddIsPausedToUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsPaused",
                table: "Users",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsPaused",
                table: "Users");
        }
    }
}
