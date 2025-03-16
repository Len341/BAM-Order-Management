using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BA.OrderScraper.Migrations
{
    /// <inheritdoc />
    public partial class UpdateKey_Manifest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_StagingManifest",
                table: "StagingManifest");

            migrationBuilder.AlterColumn<string>(
                name: "SupplierKanbanNumber",
                table: "StagingManifest",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "SupplierManifestNo",
                table: "StagingManifest",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddPrimaryKey(
                name: "PK_StagingManifest",
                table: "StagingManifest",
                column: "SupplierKanbanNumber");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_StagingManifest",
                table: "StagingManifest");

            migrationBuilder.AlterColumn<string>(
                name: "SupplierManifestNo",
                table: "StagingManifest",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "SupplierKanbanNumber",
                table: "StagingManifest",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddPrimaryKey(
                name: "PK_StagingManifest",
                table: "StagingManifest",
                column: "SupplierManifestNo");
        }
    }
}
