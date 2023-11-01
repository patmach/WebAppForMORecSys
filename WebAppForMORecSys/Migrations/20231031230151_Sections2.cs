using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebAppForMORecSys.Migrations
{
    /// <inheritdoc />
    public partial class Sections2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "QuestionSectionID",
                table: "Questions",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Questions_QuestionSectionID",
                table: "Questions",
                column: "QuestionSectionID");

            migrationBuilder.AddForeignKey(
                name: "FK_Questions_QuestionSections_QuestionSectionID",
                table: "Questions",
                column: "QuestionSectionID",
                principalTable: "QuestionSections",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Questions_QuestionSections_QuestionSectionID",
                table: "Questions");

            migrationBuilder.DropIndex(
                name: "IX_Questions_QuestionSectionID",
                table: "Questions");

            migrationBuilder.AlterColumn<int>(
                name: "QuestionSectionID",
                table: "Questions",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");
        }
    }
}
