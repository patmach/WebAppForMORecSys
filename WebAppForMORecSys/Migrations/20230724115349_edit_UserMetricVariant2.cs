using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebAppForMORecSys.Migrations
{
    /// <inheritdoc />
    public partial class edit_UserMetricVariant2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UsersMetrics_MetricVariants_MetricVariantID",
                table: "UsersMetrics");

            migrationBuilder.DropForeignKey(
                name: "FK_UsersMetrics_Metrics_MetricId",
                table: "UsersMetrics");

            migrationBuilder.DropForeignKey(
                name: "FK_UsersMetrics_Users_UserID",
                table: "UsersMetrics");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UsersMetrics",
                table: "UsersMetrics");

            migrationBuilder.RenameTable(
                name: "UsersMetrics",
                newName: "UserMetricVariants");

            migrationBuilder.RenameIndex(
                name: "IX_UsersMetrics_UserID",
                table: "UserMetricVariants",
                newName: "IX_UserMetricVariants_UserID");

            migrationBuilder.RenameIndex(
                name: "IX_UsersMetrics_MetricVariantID",
                table: "UserMetricVariants",
                newName: "IX_UserMetricVariants_MetricVariantID");

            migrationBuilder.RenameIndex(
                name: "IX_UsersMetrics_MetricId",
                table: "UserMetricVariants",
                newName: "IX_UserMetricVariants_MetricId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserMetricVariants",
                table: "UserMetricVariants",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_UserMetricVariants_MetricVariants_MetricVariantID",
                table: "UserMetricVariants",
                column: "MetricVariantID",
                principalTable: "MetricVariants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserMetricVariants_Metrics_MetricId",
                table: "UserMetricVariants",
                column: "MetricId",
                principalTable: "Metrics",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_UserMetricVariants_Users_UserID",
                table: "UserMetricVariants",
                column: "UserID",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserMetricVariants_MetricVariants_MetricVariantID",
                table: "UserMetricVariants");

            migrationBuilder.DropForeignKey(
                name: "FK_UserMetricVariants_Metrics_MetricId",
                table: "UserMetricVariants");

            migrationBuilder.DropForeignKey(
                name: "FK_UserMetricVariants_Users_UserID",
                table: "UserMetricVariants");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UserMetricVariants",
                table: "UserMetricVariants");

            migrationBuilder.RenameTable(
                name: "UserMetricVariants",
                newName: "UsersMetrics");

            migrationBuilder.RenameIndex(
                name: "IX_UserMetricVariants_UserID",
                table: "UsersMetrics",
                newName: "IX_UsersMetrics_UserID");

            migrationBuilder.RenameIndex(
                name: "IX_UserMetricVariants_MetricVariantID",
                table: "UsersMetrics",
                newName: "IX_UsersMetrics_MetricVariantID");

            migrationBuilder.RenameIndex(
                name: "IX_UserMetricVariants_MetricId",
                table: "UsersMetrics",
                newName: "IX_UsersMetrics_MetricId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UsersMetrics",
                table: "UsersMetrics",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_UsersMetrics_MetricVariants_MetricVariantID",
                table: "UsersMetrics",
                column: "MetricVariantID",
                principalTable: "MetricVariants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UsersMetrics_Metrics_MetricId",
                table: "UsersMetrics",
                column: "MetricId",
                principalTable: "Metrics",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_UsersMetrics_Users_UserID",
                table: "UsersMetrics",
                column: "UserID",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
