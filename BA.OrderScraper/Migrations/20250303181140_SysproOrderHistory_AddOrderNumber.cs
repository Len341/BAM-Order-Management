using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BA.OrderScraper.Migrations
{
    /// <inheritdoc />
    public partial class SysproOrderHistory_AddOrderNumber : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "OrderNumber",
                table: "SysproOrderCreationHistory",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OrderNumber",
                table: "SysproOrderCreationHistory");
        }
    }
}
