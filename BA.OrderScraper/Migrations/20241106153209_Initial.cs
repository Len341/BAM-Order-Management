using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BA.OrderScraper.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "StagingKanbanCard",
                columns: table => new
                {
                    KanbanID = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    SupplierName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SupplierNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SupplierPartNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ArrivalDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Shop = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PartName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MaterialNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    KanbanNo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OrderNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BinTypeCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Qty = table.Column<int>(type: "int", nullable: false),
                    KanbanPrintAddress1 = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ReceivingDock = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ProgressLane = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OnDockRoute = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ManifestNumber = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StagingKanbanCard", x => x.KanbanID);
                });

            migrationBuilder.CreateTable(
                name: "StagingManifest",
                columns: table => new
                {
                    SupplierManifestNo = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    SupplierName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SupplierAddress = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    City = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Region = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PostCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SupplierNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ShipDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ShipTime = table.Column<TimeSpan>(type: "time", nullable: false),
                    PickupRoute = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SupplierRoute = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SupplierArriveDock = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SupplierDepartDock = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    JHB_PE = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SupplierArriveDocJHBPE = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SupplierDepartDockJHBPE = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SupplierOnDockRoute = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SupplierArrivalDateParsed = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SupplierArrivalTime = table.Column<TimeSpan>(type: "time", nullable: false),
                    SupplierProglane = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SupplierShop = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SupplierReceivingDock = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SupplierOrderNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SupplierKanbanNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SupplierMaterialNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SupplierPadEasyReferenceNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SupplierPadBinTypeCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SupplierQty = table.Column<int>(type: "int", nullable: false),
                    SupplierPurchasingDocumentNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SupplierBinReq = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StagingManifest", x => x.SupplierManifestNo);
                });

            migrationBuilder.CreateTable(
                name: "StagingSkid",
                columns: table => new
                {
                    ManifestNumber = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    SupplierCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SupplierName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ReceivingDock = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ProgressLane = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OrderNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PickupRoute = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DepartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Route = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    XDoc2ArrivalDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    XDoc2ArrivalTime = table.Column<TimeSpan>(type: "time", nullable: false),
                    DepartDate2 = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DepartDate3 = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DepartTime = table.Column<TimeSpan>(type: "time", nullable: false),
                    OnDockRoute = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ArrivalDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ArrivalTime = table.Column<TimeSpan>(type: "time", nullable: false),
                    TsamDepartDocDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TsamDepartDocTime = table.Column<TimeSpan>(type: "time", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StagingSkid", x => x.ManifestNumber);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StagingKanbanCard");

            migrationBuilder.DropTable(
                name: "StagingManifest");

            migrationBuilder.DropTable(
                name: "StagingSkid");
        }
    }
}
