using Microsoft.EntityFrameworkCore;
using Ajit_Bakery.Models;

namespace Ajit_Bakery.Data
{
    public class DataDBContext : DbContext
    {
        public DataDBContext(DbContextOptions<DataDBContext> options) : base(options) 
        { 
        }
        public DataDBContext()
        {
        }
        //MASTERS
        public DbSet<Ajit_Bakery.Models.ProductMaster> ProductMaster { get; set; } 
        public DbSet<Ajit_Bakery.Models.DialMaster> DialMaster { get; set; } 
        public DbSet<Ajit_Bakery.Models.BoxMaster> BoxMaster { get; set; } 
        public DbSet<Ajit_Bakery.Models.OutletMaster> OutletMaster { get; set; } 
        public DbSet<Ajit_Bakery.Models.TransportMaster> TransportMaster { get; set; } 
        public DbSet<Ajit_Bakery.Models.UserMaster> UserMaster { get; set; } 
        public DbSet<Ajit_Bakery.Models.MenuModel> MenuModel { get; set; } 
        public DbSet<Ajit_Bakery.Models.UserManagment> UserManagment { get; set; } 
        public DbSet<Ajit_Bakery.Models.UserType> UserType { get; set; } = default!;
        //OPERATIONS
        public DbSet<Ajit_Bakery.Models.ProductionCapture> ProductionCapture { get; set; } 
        public DbSet<Ajit_Bakery.Models.SaveProduction> SaveProduction { get; set; }
        public DbSet<Ajit_Bakery.Models.Packaging> Packaging { get; set; }
        //STICKER
        public DbSet<Ajit_Bakery.Models.Sticker> Sticker { get; set; }
        //ID TO GENERATE PRODUCTION 
        public DbSet<Ajit_Bakery.Models.ProductionIds> ProductionIds { get; set; }
    }
}
