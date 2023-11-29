using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebAppForMORecSys.Migrations
{
    /// <inheritdoc />
    public partial class UserActModification : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Done",
                table: "UserActs");

            migrationBuilder.DropColumn(
                name: "NumberOfSuggestion",
                table: "UserActs");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Done",
                table: "UserActs",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "NumberOfSuggestion",
                table: "UserActs",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
