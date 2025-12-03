using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DriveTrack.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddCreatedByNameToExpenseAndReminder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CreatedByName",
                table: "Reminders",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CreatedByName",
                table: "Expenses",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedByName",
                table: "Reminders");

            migrationBuilder.DropColumn(
                name: "CreatedByName",
                table: "Expenses");
        }
    }
}
