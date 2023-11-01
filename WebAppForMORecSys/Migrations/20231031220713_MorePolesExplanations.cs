using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebAppForMORecSys.Migrations
{
    /// <inheritdoc />
    public partial class MorePolesExplanations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "NegativeExplanation",
                table: "MetricVariants",
                newName: "VeryNegativeExplanation");

            migrationBuilder.RenameColumn(
                name: "Explanation",
                table: "MetricVariants",
                newName: "VeryPositiveExplanation");

            migrationBuilder.RenameColumn(
                name: "DefaultNegativeExplanation",
                table: "Metrics",
                newName: "DefaultVeryPositiveExplanation");

            migrationBuilder.RenameColumn(
                name: "DefaultExplanation",
                table: "Metrics",
                newName: "DefaultVeryNegativeExplanation");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "VeryPositiveExplanation",
                table: "MetricVariants",
                newName: "Explanation");

            migrationBuilder.RenameColumn(
                name: "VeryNegativeExplanation",
                table: "MetricVariants",
                newName: "NegativeExplanation");

            migrationBuilder.RenameColumn(
                name: "DefaultVeryPositiveExplanation",
                table: "Metrics",
                newName: "DefaultNegativeExplanation");

            migrationBuilder.RenameColumn(
                name: "DefaultVeryNegativeExplanation",
                table: "Metrics",
                newName: "DefaultExplanation");
        }
    }
}
