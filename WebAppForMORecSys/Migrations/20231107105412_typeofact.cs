using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebAppForMORecSys.Migrations
{
    /// <inheritdoc />
    public partial class typeofact : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserActSuggestion_Acts_ActID",
                table: "UserActSuggestion");

            migrationBuilder.DropForeignKey(
                name: "FK_UserActSuggestion_Users_UserID",
                table: "UserActSuggestion");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UserActSuggestion",
                table: "UserActSuggestion");

            migrationBuilder.RenameTable(
                name: "UserActSuggestion",
                newName: "UserActSuggestions");

            migrationBuilder.RenameColumn(
                name: "NumberOfSuggestion",
                table: "UserActSuggestions",
                newName: "NumberOfSuggestions");

            migrationBuilder.RenameIndex(
                name: "IX_UserActSuggestion_UserID",
                table: "UserActSuggestions",
                newName: "IX_UserActSuggestions_UserID");

            migrationBuilder.RenameIndex(
                name: "IX_UserActSuggestion_ActID",
                table: "UserActSuggestions",
                newName: "IX_UserActSuggestions_ActID");

            migrationBuilder.AddColumn<string>(
                name: "TypeOfAct",
                table: "Acts",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserActSuggestions",
                table: "UserActSuggestions",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_UserActSuggestions_Acts_ActID",
                table: "UserActSuggestions",
                column: "ActID",
                principalTable: "Acts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserActSuggestions_Users_UserID",
                table: "UserActSuggestions",
                column: "UserID",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserActSuggestions_Acts_ActID",
                table: "UserActSuggestions");

            migrationBuilder.DropForeignKey(
                name: "FK_UserActSuggestions_Users_UserID",
                table: "UserActSuggestions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UserActSuggestions",
                table: "UserActSuggestions");

            migrationBuilder.DropColumn(
                name: "TypeOfAct",
                table: "Acts");

            migrationBuilder.RenameTable(
                name: "UserActSuggestions",
                newName: "UserActSuggestion");

            migrationBuilder.RenameColumn(
                name: "NumberOfSuggestions",
                table: "UserActSuggestion",
                newName: "NumberOfSuggestion");

            migrationBuilder.RenameIndex(
                name: "IX_UserActSuggestions_UserID",
                table: "UserActSuggestion",
                newName: "IX_UserActSuggestion_UserID");

            migrationBuilder.RenameIndex(
                name: "IX_UserActSuggestions_ActID",
                table: "UserActSuggestion",
                newName: "IX_UserActSuggestion_ActID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserActSuggestion",
                table: "UserActSuggestion",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_UserActSuggestion_Acts_ActID",
                table: "UserActSuggestion",
                column: "ActID",
                principalTable: "Acts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserActSuggestion_Users_UserID",
                table: "UserActSuggestion",
                column: "UserID",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
