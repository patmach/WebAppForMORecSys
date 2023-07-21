using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebAppForMORecSys.Migrations
{
    /// <inheritdoc />
    public partial class MetricVariant3 : Migration
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

            migrationBuilder.AddColumn<int>(
                name: "MetricVariantID",
                table: "UsersMetrics",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "MetricVariants",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MetricID = table.Column<int>(type: "int", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DefaultVariant = table.Column<bool>(type: "bit", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Explanation = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MetricVariants", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MetricVariants_Metrics_MetricID",
                        column: x => x.MetricID,
                        principalTable: "Metrics",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UsersMetrics_MetricVariantID",
                table: "UsersMetrics",
                column: "MetricVariantID");

            migrationBuilder.CreateIndex(
                name: "IX_MetricVariants_MetricID",
                table: "MetricVariants",
                column: "MetricID");

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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UsersMetrics_MetricVariants_MetricVariantID",
                table: "UsersMetrics");

            migrationBuilder.DropForeignKey(
                name: "FK_UsersMetrics_Metrics_MetricId",
                table: "UsersMetrics");

            migrationBuilder.DropTable(
                name: "MetricVariants");

            migrationBuilder.DropIndex(
                name: "IX_UsersMetrics_MetricVariantID",
                table: "UsersMetrics");

            migrationBuilder.DropColumn(
                name: "MetricVariantID",
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
