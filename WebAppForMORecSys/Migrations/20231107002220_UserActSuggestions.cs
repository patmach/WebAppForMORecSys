using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebAppForMORecSys.Migrations
{
    /// <inheritdoc />
    public partial class UserActSuggestions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "FirstRecommendationTime",
                table: "Users",
                type: "datetime2",
                nullable: true);

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

            migrationBuilder.CreateTable(
                name: "UserActSuggestion",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ActID = table.Column<int>(type: "int", nullable: false),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    NumberOfSuggestion = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserActSuggestion", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserActSuggestion_Acts_ActID",
                        column: x => x.ActID,
                        principalTable: "Acts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserActSuggestion_Users_UserID",
                        column: x => x.UserID,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserActSuggestion_ActID",
                table: "UserActSuggestion",
                column: "ActID");

            migrationBuilder.CreateIndex(
                name: "IX_UserActSuggestion_UserID",
                table: "UserActSuggestion",
                column: "UserID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserActSuggestion");

            migrationBuilder.DropColumn(
                name: "FirstRecommendationTime",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Done",
                table: "UserActs");

            migrationBuilder.DropColumn(
                name: "NumberOfSuggestion",
                table: "UserActs");
        }
    }
}
