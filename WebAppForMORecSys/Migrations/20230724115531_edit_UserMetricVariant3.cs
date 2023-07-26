using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebAppForMORecSys.Migrations
{
    /// <inheritdoc />
    public partial class edit_UserMetricVariant3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserMetricVariants_Metrics_MetricId",
                table: "UserMetricVariants");

            migrationBuilder.DropIndex(
                name: "IX_UserMetricVariants_MetricId",
                table: "UserMetricVariants");

            migrationBuilder.DropColumn(
                name: "MetricId",
                table: "UserMetricVariants");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MetricId",
                table: "UserMetricVariants",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserMetricVariants_MetricId",
                table: "UserMetricVariants",
                column: "MetricId");

            migrationBuilder.AddForeignKey(
                name: "FK_UserMetricVariants_Metrics_MetricId",
                table: "UserMetricVariants",
                column: "MetricId",
                principalTable: "Metrics",
                principalColumn: "Id");
        }
    }
}
