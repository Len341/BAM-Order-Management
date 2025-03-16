using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BA.OrderScraper.Migrations
{
    /// <inheritdoc />
    public partial class Update_SysproOrderCreationHistory_RemovePK : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_SysproOrderCreationHistory",
                table: "SysproOrderCreationHistory");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddPrimaryKey(
                name: "PK_SysproOrderCreationHistory",
                table: "SysproOrderCreationHistory",
                column: "ManifestNumber");
        }
    }
}
