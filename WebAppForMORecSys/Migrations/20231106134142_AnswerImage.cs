using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebAppForMORecSys.Migrations
{
    /// <inheritdoc />
    public partial class AnswerImage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "HasImage",
                table: "Answers",
                type: "bit",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HasImage",
                table: "Answers");
        }
    }
}
