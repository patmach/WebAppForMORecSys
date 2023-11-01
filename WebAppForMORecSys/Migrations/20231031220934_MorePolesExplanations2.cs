using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebAppForMORecSys.Migrations
{
    /// <inheritdoc />
    public partial class MorePolesExplanations2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "RatherNegativeExplanation",
                table: "MetricVariants",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RatherPositiveExplanation",
                table: "MetricVariants",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DefaultRatherNegativeExplanation",
                table: "Metrics",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DefaultRatherPositiveExplanation",
                table: "Metrics",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RatherNegativeExplanation",
                table: "MetricVariants");

            migrationBuilder.DropColumn(
                name: "RatherPositiveExplanation",
                table: "MetricVariants");

            migrationBuilder.DropColumn(
                name: "DefaultRatherNegativeExplanation",
                table: "Metrics");

            migrationBuilder.DropColumn(
                name: "DefaultRatherPositiveExplanation",
                table: "Metrics");
        }
    }
}
