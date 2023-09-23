using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TalentBay1.Migrations
{
    /// <inheritdoc />
    public partial class EverythingWorksButEveryAssignmentisSame : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsCompleted",
                table: "Assignment",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsCompleted",
                table: "Assignment");
        }
    }
}
