using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BA.OrderScraper.Migrations
{
    /// <inheritdoc />
    public partial class ManifestAddCompositePK : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "SupplierPadEasyReferenceNumber",
                table: "StagingManifest",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "SupplierKanbanNumber",
                table: "StagingManifest",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddPrimaryKey(
                name: "PK_StagingManifest",
                table: "StagingManifest",
                columns: new[] { "SupplierManifestNo", "SupplierKanbanNumber", "SupplierPadEasyReferenceNumber" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_StagingManifest",
                table: "StagingManifest");

            migrationBuilder.AlterColumn<string>(
                name: "SupplierPadEasyReferenceNumber",
                table: "StagingManifest",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "SupplierKanbanNumber",
                table: "StagingManifest",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");
        }
    }
}
