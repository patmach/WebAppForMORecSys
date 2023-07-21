using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebAppForMORecSys.Migrations
{
    /// <inheritdoc />
    public partial class MetricVariant1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UsersMetrics_Metrics_MetricID",
                table: "UsersMetrics");

            migrationBuilder.RenameColumn(
                name: "MetricID",
                table: "UsersMetrics",
                newName: "MetricId");

            migrationBuilder.RenameIndex(
                name: "IX_UsersMetrics_MetricID",
                table: "UsersMetrics",
                newName: "IX_UsersMetrics_MetricId");

            migrationBuilder.AlterColumn<int>(
                name: "MetricId",
                table: "UsersMetrics",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_UsersMetrics_Metrics_MetricId",
                table: "UsersMetrics",
                column: "MetricId",
                principalTable: "Metrics",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UsersMetrics_Metrics_MetricId",
                table: "UsersMetrics");

            migrationBuilder.RenameColumn(
                name: "MetricId",
                table: "UsersMetrics",
                newName: "MetricID");

            migrationBuilder.RenameIndex(
                name: "IX_UsersMetrics_MetricId",
                table: "UsersMetrics",
                newName: "IX_UsersMetrics_MetricID");

            migrationBuilder.AlterColumn<int>(
                name: "MetricID",
                table: "UsersMetrics",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_UsersMetrics_Metrics_MetricID",
                table: "UsersMetrics",
                column: "MetricID",
                principalTable: "Metrics",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
