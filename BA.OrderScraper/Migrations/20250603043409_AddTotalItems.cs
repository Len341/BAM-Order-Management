using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BA.OrderScraper.Migrations
{
    /// <inheritdoc />
    public partial class AddTotalItems : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "OrderTotalItems",
                table: "SysproOrderCreationHistory",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "OrderTotalItemsCompleted",
                table: "SysproOrderCreationHistory",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OrderTotalItems",
                table: "SysproOrderCreationHistory");

            migrationBuilder.DropColumn(
                name: "OrderTotalItemsCompleted",
                table: "SysproOrderCreationHistory");
        }
    }
}
