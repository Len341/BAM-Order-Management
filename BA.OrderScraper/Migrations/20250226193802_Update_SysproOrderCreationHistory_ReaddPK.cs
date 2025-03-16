using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BA.OrderScraper.Migrations
{
    /// <inheritdoc />
    public partial class Update_SysproOrderCreationHistory_ReaddPK : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "Id",
                table: "SysproOrderCreationHistory",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddPrimaryKey(
                name: "PK_SysproOrderCreationHistory",
                table: "SysproOrderCreationHistory",
                column: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_SysproOrderCreationHistory",
                table: "SysproOrderCreationHistory");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "SysproOrderCreationHistory");
        }
    }
}
