using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Ajit_Bakery.Migrations
{
    /// <inheritdoc />
    public partial class BB : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            //migrationBuilder.CreateTable(
            //    name: "BoxMaster",
            //    columns: table => new
            //    {
            //        Id = table.Column<int>(type: "integer", nullable: false)
            //            .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
            //        BoxNumber = table.Column<string>(type: "text", nullable: false),
            //        BoxLength = table.Column<double>(type: "double precision", nullable: false),
            //        BoxBreadth = table.Column<double>(type: "double precision", nullable: false),
            //        BoxHeight = table.Column<double>(type: "double precision", nullable: false),
            //        BoxUom = table.Column<string>(type: "text", nullable: false),
            //        BoxArea = table.Column<string>(type: "text", nullable: true),
            //        CreateDate = table.Column<string>(type: "text", nullable: true),
            //        Createtime = table.Column<string>(type: "text", nullable: true),
            //        ModifiedDate = table.Column<string>(type: "text", nullable: true),
            //        Modifiedtime = table.Column<string>(type: "text", nullable: true),
            //        User = table.Column<string>(type: "text", nullable: true)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_BoxMaster", x => x.Id);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "DialMaster",
            //    columns: table => new
            //    {
            //        Id = table.Column<int>(type: "integer", nullable: false)
            //            .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
            //        DialCode = table.Column<string>(type: "text", nullable: false),
            //        DialShape = table.Column<string>(type: "text", nullable: false),
            //        DialWg = table.Column<double>(type: "double precision", nullable: false),
            //        DialWgUom = table.Column<string>(type: "text", nullable: false),
            //        DialDiameter = table.Column<double>(type: "double precision", nullable: false),
            //        DialLength = table.Column<double>(type: "double precision", nullable: false),
            //        DialBreadth = table.Column<double>(type: "double precision", nullable: false),
            //        LengthUom = table.Column<string>(type: "text", nullable: false),
            //        DialArea = table.Column<string>(type: "text", nullable: true),
            //        CreateDate = table.Column<string>(type: "text", nullable: true),
            //        Createtime = table.Column<string>(type: "text", nullable: true),
            //        ModifiedDate = table.Column<string>(type: "text", nullable: true),
            //        Modifiedtime = table.Column<string>(type: "text", nullable: true),
            //        User = table.Column<string>(type: "text", nullable: true)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_DialMaster", x => x.Id);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "MenuModel",
            //    columns: table => new
            //    {
            //        MenuId = table.Column<int>(type: "integer", nullable: false)
            //            .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
            //        ParentMenuId = table.Column<int>(type: "integer", nullable: true),
            //        icon = table.Column<string>(type: "text", nullable: true),
            //        Title = table.Column<string>(type: "text", nullable: false),
            //        Controller = table.Column<string>(type: "text", nullable: false),
            //        Action = table.Column<string>(type: "text", nullable: false)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_MenuModel", x => x.MenuId);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "OutletMaster",
            //    columns: table => new
            //    {
            //        Id = table.Column<int>(type: "integer", nullable: false)
            //            .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
            //        OutletCode = table.Column<string>(type: "text", nullable: false),
            //        OutletName = table.Column<string>(type: "text", nullable: false),
            //        OutletAddress = table.Column<string>(type: "text", nullable: false),
            //        OutletContactNo = table.Column<string>(type: "text", nullable: false),
            //        OutletContactPerson = table.Column<string>(type: "text", nullable: false),
            //        CreateDate = table.Column<string>(type: "text", nullable: true),
            //        Createtime = table.Column<string>(type: "text", nullable: true),
            //        ModifiedDate = table.Column<string>(type: "text", nullable: true),
            //        Modifiedtime = table.Column<string>(type: "text", nullable: true),
            //        User = table.Column<string>(type: "text", nullable: true)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_OutletMaster", x => x.Id);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "ProductionCapture",
            //    columns: table => new
            //    {
            //        Id = table.Column<int>(type: "integer", nullable: false)
            //            .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
            //        Production_Id = table.Column<string>(type: "text", nullable: false),
            //        ProductName = table.Column<string>(type: "text", nullable: false),
            //        Unit = table.Column<string>(type: "text", nullable: false),
            //        OutletName = table.Column<string>(type: "text", nullable: false),
            //        TotalQty = table.Column<int>(type: "integer", nullable: false),
            //        Production_Date = table.Column<string>(type: "text", nullable: false),
            //        Production_Time = table.Column<string>(type: "text", nullable: false)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_ProductionCapture", x => x.Id);
            //    });

            migrationBuilder.CreateTable(
                name: "ProductionIds",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ProductionId = table.Column<string>(type: "text", nullable: false),
                    date = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductionIds", x => x.id);
                });

            //migrationBuilder.CreateTable(
            //    name: "ProductMaster",
            //    columns: table => new
            //    {
            //        Id = table.Column<int>(type: "integer", nullable: false)
            //            .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
            //        ProductCode = table.Column<string>(type: "text", nullable: false),
            //        ProductName = table.Column<string>(type: "text", nullable: false),
            //        Qty = table.Column<int>(type: "integer", nullable: false),
            //        Unitqty = table.Column<int>(type: "integer", nullable: false),
            //        Uom = table.Column<string>(type: "text", nullable: false),
            //        Category = table.Column<string>(type: "text", nullable: false),
            //        Type = table.Column<string>(type: "text", nullable: false),
            //        PerGmRate = table.Column<string>(type: "text", nullable: false),
            //        MRP = table.Column<double>(type: "double precision", nullable: false),
            //        CreateDate = table.Column<string>(type: "text", nullable: true),
            //        Createtime = table.Column<string>(type: "text", nullable: true),
            //        ModifiedDate = table.Column<string>(type: "text", nullable: true),
            //        Modifiedtime = table.Column<string>(type: "text", nullable: true),
            //        User = table.Column<string>(type: "text", nullable: true),
            //        Dial = table.Column<string>(type: "text", nullable: true)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_ProductMaster", x => x.Id);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "SaveProduction",
            //    columns: table => new
            //    {
            //        Id = table.Column<int>(type: "integer", nullable: false)
            //            .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
            //        ProductName = table.Column<string>(type: "text", nullable: false),
            //        ProductGrossWg = table.Column<double>(type: "double precision", nullable: false),
            //        DialTierWg = table.Column<double>(type: "double precision", nullable: false),
            //        DialTierWg_Uom = table.Column<string>(type: "text", nullable: false),
            //        DialCode = table.Column<string>(type: "text", nullable: false),
            //        TotalNetWg = table.Column<double>(type: "double precision", nullable: false),
            //        TotalNetWg_Uom = table.Column<string>(type: "text", nullable: false),
            //        Qty = table.Column<int>(type: "integer", nullable: false),
            //        SaveProduction_Date = table.Column<string>(type: "text", nullable: false),
            //        SaveProduction_Time = table.Column<string>(type: "text", nullable: false)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_SaveProduction", x => x.Id);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "TransportMaster",
            //    columns: table => new
            //    {
            //        Id = table.Column<int>(type: "integer", nullable: false)
            //            .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
            //        DriverName = table.Column<string>(type: "text", nullable: false),
            //        DriverContactNo = table.Column<string>(type: "text", nullable: false),
            //        VehicleNo = table.Column<string>(type: "text", nullable: false),
            //        VehicleOwn = table.Column<string>(type: "text", nullable: false),
            //        CreateDate = table.Column<string>(type: "text", nullable: true),
            //        Createtime = table.Column<string>(type: "text", nullable: true),
            //        ModifiedDate = table.Column<string>(type: "text", nullable: true),
            //        Modifiedtime = table.Column<string>(type: "text", nullable: true),
            //        User = table.Column<string>(type: "text", nullable: true)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_TransportMaster", x => x.Id);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "UserManagment",
            //    columns: table => new
            //    {
            //        Id = table.Column<int>(type: "integer", nullable: false)
            //            .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
            //        UserName = table.Column<string>(type: "text", nullable: false),
            //        PageName = table.Column<string>(type: "text", nullable: false),
            //        Role = table.Column<string>(type: "text", nullable: false)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_UserManagment", x => x.Id);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "UserMaster",
            //    columns: table => new
            //    {
            //        Id = table.Column<int>(type: "integer", nullable: false)
            //            .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
            //        UserName = table.Column<string>(type: "text", nullable: false),
            //        Email = table.Column<string>(type: "text", nullable: false),
            //        UserPassward = table.Column<string>(type: "text", nullable: false),
            //        UserCode = table.Column<string>(type: "text", nullable: false),
            //        UserDept = table.Column<string>(type: "text", nullable: false),
            //        UserContactNo = table.Column<string>(type: "text", nullable: false),
            //        UserRole = table.Column<string>(type: "text", nullable: false),
            //        KeepLoogedIn = table.Column<bool>(type: "boolean", nullable: false),
            //        CreateDate = table.Column<string>(type: "text", nullable: true),
            //        Createtime = table.Column<string>(type: "text", nullable: true),
            //        ModifiedDate = table.Column<string>(type: "text", nullable: true),
            //        Modifiedtime = table.Column<string>(type: "text", nullable: true)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_UserMaster", x => x.Id);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "UserType",
            //    columns: table => new
            //    {
            //        user_id = table.Column<int>(type: "integer", nullable: false)
            //            .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
            //        usertype_name = table.Column<string>(type: "text", nullable: false),
            //        designation = table.Column<string>(type: "text", nullable: false)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_UserType", x => x.user_id);
            //    });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            //migrationBuilder.DropTable(
            //    name: "BoxMaster");

            //migrationBuilder.DropTable(
            //    name: "DialMaster");

            //migrationBuilder.DropTable(
            //    name: "MenuModel");

            //migrationBuilder.DropTable(
            //    name: "OutletMaster");

            //migrationBuilder.DropTable(
            //    name: "ProductionCapture");

            migrationBuilder.DropTable(
                name: "ProductionIds");

            //migrationBuilder.DropTable(
            //    name: "ProductMaster");

            //migrationBuilder.DropTable(
            //    name: "SaveProduction");

            //migrationBuilder.DropTable(
            //    name: "TransportMaster");

            //migrationBuilder.DropTable(
            //    name: "UserManagment");

            //migrationBuilder.DropTable(
            //    name: "UserMaster");

            //migrationBuilder.DropTable(
            //    name: "UserType");
        }
    }
}
