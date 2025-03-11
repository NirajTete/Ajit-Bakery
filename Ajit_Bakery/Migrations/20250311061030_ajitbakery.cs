using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Ajit_Bakery.Migrations
{
    /// <inheritdoc />
    public partial class ajitbakery : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BoxMaster",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    BoxNumber = table.Column<string>(type: "text", nullable: false),
                    BoxLength = table.Column<double>(type: "double precision", nullable: false),
                    BoxBreadth = table.Column<double>(type: "double precision", nullable: false),
                    BoxHeight = table.Column<double>(type: "double precision", nullable: false),
                    BoxUom = table.Column<string>(type: "text", nullable: false),
                    BoxArea = table.Column<string>(type: "text", nullable: true),
                    CreateDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Createtime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Modifiedtime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    User = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BoxMaster", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DialMaster",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DialCode = table.Column<string>(type: "text", nullable: false),
                    DialShape = table.Column<string>(type: "text", nullable: false),
                    DialWg = table.Column<double>(type: "double precision", nullable: false),
                    DialWgUom = table.Column<string>(type: "text", nullable: false),
                    DialDiameter = table.Column<double>(type: "double precision", nullable: false),
                    DialLength = table.Column<double>(type: "double precision", nullable: false),
                    DialBreadth = table.Column<double>(type: "double precision", nullable: false),
                    DialUom = table.Column<string>(type: "text", nullable: false),
                    DialArea = table.Column<string>(type: "text", nullable: true),
                    CreateDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Createtime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Modifiedtime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    User = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DialMaster", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OutletMaster",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    OutletCode = table.Column<string>(type: "text", nullable: false),
                    OutletName = table.Column<string>(type: "text", nullable: false),
                    OutletAddress = table.Column<string>(type: "text", nullable: false),
                    OutletContactNo = table.Column<string>(type: "text", nullable: false),
                    OutletContactPerson = table.Column<string>(type: "text", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Createtime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Modifiedtime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    User = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OutletMaster", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProductMaster",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ProductName = table.Column<string>(type: "text", nullable: false),
                    Qty = table.Column<int>(type: "integer", nullable: false),
                    Unitqty = table.Column<int>(type: "integer", nullable: false),
                    Uom = table.Column<string>(type: "text", nullable: false),
                    Category = table.Column<string>(type: "text", nullable: false),
                    Type = table.Column<string>(type: "text", nullable: false),
                    PerGmRate = table.Column<string>(type: "text", nullable: false),
                    MRP = table.Column<double>(type: "double precision", nullable: false),
                    DialCode1 = table.Column<string>(type: "text", nullable: true),
                    DialCode2 = table.Column<string>(type: "text", nullable: true),
                    CreateDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Createtime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Modifiedtime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    User = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductMaster", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TransportMaster",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DriverName = table.Column<string>(type: "text", nullable: false),
                    DriverContactNo = table.Column<string>(type: "text", nullable: false),
                    VehicleNo = table.Column<string>(type: "text", nullable: false),
                    VehicleOwn = table.Column<string>(type: "text", nullable: false),
                    VehicleType = table.Column<string>(type: "text", nullable: false),
                    VehicleNoOfTyre = table.Column<string>(type: "text", nullable: true),
                    VehicleCapacity = table.Column<string>(type: "text", nullable: true),
                    VehicleVolume = table.Column<string>(type: "text", nullable: true),
                    CreateDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Createtime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Modifiedtime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    User = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TransportMaster", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserMaster",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserName = table.Column<string>(type: "text", nullable: false),
                    UserCode = table.Column<string>(type: "text", nullable: false),
                    UserDept = table.Column<string>(type: "text", nullable: false),
                    UserDesignation = table.Column<string>(type: "text", nullable: false),
                    UserContactNo = table.Column<string>(type: "text", nullable: false),
                    UserRole = table.Column<string>(type: "text", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Createtime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Modifiedtime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    User = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserMaster", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BoxMaster");

            migrationBuilder.DropTable(
                name: "DialMaster");

            migrationBuilder.DropTable(
                name: "OutletMaster");

            migrationBuilder.DropTable(
                name: "ProductMaster");

            migrationBuilder.DropTable(
                name: "TransportMaster");

            migrationBuilder.DropTable(
                name: "UserMaster");
        }
    }
}
