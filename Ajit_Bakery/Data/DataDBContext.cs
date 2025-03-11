using Microsoft.EntityFrameworkCore;

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

        public DbSet<Ajit_Bakery.Models.ProductMaster> ProductMaster { get; set; } 
        public DbSet<Ajit_Bakery.Models.DialMaster> DialMaster { get; set; } 
        public DbSet<Ajit_Bakery.Models.BoxMaster> BoxMaster { get; set; } 
        public DbSet<Ajit_Bakery.Models.OutletMaster> OutletMaster { get; set; } 
        public DbSet<Ajit_Bakery.Models.TransportMaster> TransportMaster { get; set; } 
        public DbSet<Ajit_Bakery.Models.UserMaster> UserMaster { get; set; } 
        public DbSet<Ajit_Bakery.Models.MenuModel> MenuModel { get; set; } 
        public DbSet<Ajit_Bakery.Models.UserManagment> UserManagment { get; set; } 
    }
}
