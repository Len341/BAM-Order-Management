﻿// <auto-generated />
using System;
using BA.OrderScraper.EFCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace BA.OrderScraper.Migrations
{
    [DbContext(typeof(BADbContext))]
    [Migration("20250304203029_ManifestAddCompositePK")]
    partial class ManifestAddCompositePK
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.10")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("BA.OrderScraper.Models.Error", b =>
                {
                    b.Property<Guid>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTime>("CreatedDate")
                        .HasColumnType("datetime2");

                    b.Property<string>("ErrorMessage")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("InnerExceptionMessage")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("RetryCount")
                        .HasColumnType("int");

                    b.Property<string>("StackTrace")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("ID");

                    b.ToTable("Error");
                });

            modelBuilder.Entity("BA.OrderScraper.Models.KanbanCard", b =>
                {
                    b.Property<string>("KanbanID")
                        .HasColumnType("nvarchar(450)");

                    b.Property<DateTime>("ArrivalDate")
                        .HasColumnType("datetime2");

                    b.Property<string>("BinTypeCode")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("KanbanNo")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("KanbanPrintAddress1")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ManifestNumber")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("MaterialNumber")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("OnDockRoute")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("OrderNumber")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("PartName")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ProgressLane")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("Qty")
                        .HasColumnType("int");

                    b.Property<string>("ReceivingDock")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Shop")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("SupplierName")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("SupplierNumber")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("SupplierPartNumber")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("KanbanID");

                    b.ToTable("StagingKanbanCard");
                });

            modelBuilder.Entity("BA.OrderScraper.Models.Manifest", b =>
                {
                    b.Property<int>("SupplierManifestNo")
                        .HasColumnType("int");

                    b.Property<string>("SupplierKanbanNumber")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("SupplierPadEasyReferenceNumber")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("City")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("ImportTime")
                        .HasColumnType("datetime2");

                    b.Property<string>("JHB_PE")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("PickupRoute")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("PostCode")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Region")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("ShipDate")
                        .HasColumnType("datetime2");

                    b.Property<TimeSpan>("ShipTime")
                        .HasColumnType("time");

                    b.Property<string>("SupplierAddress")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("SupplierArrivalDateParsed")
                        .HasColumnType("datetime2");

                    b.Property<TimeSpan>("SupplierArrivalTime")
                        .HasColumnType("time");

                    b.Property<string>("SupplierArriveDocJHBPE")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("SupplierArriveDock")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("SupplierBinReq")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("SupplierDepartDock")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("SupplierDepartDockJHBPE")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("SupplierMaterialNumber")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("SupplierName")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("SupplierNumber")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("SupplierOnDockRoute")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("SupplierOrderNumber")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("SupplierPadBinTypeCode")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("SupplierProglane")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("SupplierPurchasingDocumentNumber")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<long>("SupplierQty")
                        .HasColumnType("bigint");

                    b.Property<string>("SupplierReceivingDock")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("SupplierRoute")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("SupplierShop")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("SupplierManifestNo", "SupplierKanbanNumber", "SupplierPadEasyReferenceNumber");

                    b.ToTable("StagingManifest");
                });

            modelBuilder.Entity("BA.OrderScraper.Models.Skid", b =>
                {
                    b.Property<Guid>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTime>("ArrivalDate")
                        .HasColumnType("datetime2");

                    b.Property<TimeSpan>("ArrivalTime")
                        .HasColumnType("time");

                    b.Property<DateTime>("DepartDate")
                        .HasColumnType("datetime2");

                    b.Property<DateTime>("DepartDate2")
                        .HasColumnType("datetime2");

                    b.Property<DateTime>("DepartDate3")
                        .HasColumnType("datetime2");

                    b.Property<TimeSpan>("DepartTime")
                        .HasColumnType("time");

                    b.Property<string>("ManifestNumber")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("OnDockRoute")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("OrderNumber")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("PickupRoute")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ProgressLane")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ReceivingDock")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Route")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("SupplierCode")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("SupplierName")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("TsamDepartDocDate")
                        .HasColumnType("datetime2");

                    b.Property<TimeSpan>("TsamDepartDocTime")
                        .HasColumnType("time");

                    b.Property<DateTime>("XDoc2ArrivalDate")
                        .HasColumnType("datetime2");

                    b.Property<TimeSpan>("XDoc2ArrivalTime")
                        .HasColumnType("time");

                    b.HasKey("ID");

                    b.ToTable("StagingSkid");
                });

            modelBuilder.Entity("BA.OrderScraper.Models.SysproOrderCreationHistory", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<bool>("CreationSuccess")
                        .HasColumnType("bit");

                    b.Property<string>("ErrorMessage")
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("InProgress")
                        .HasColumnType("bit");

                    b.Property<int>("ManifestNumber")
                        .HasColumnType("int");

                    b.Property<string>("OrderNumber")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("UpdatedDate")
                        .HasColumnType("datetime2");

                    b.HasKey("Id");

                    b.ToTable("SysproOrderCreationHistory");
                });
#pragma warning restore 612, 618
        }
    }
}
