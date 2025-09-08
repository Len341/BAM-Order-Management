using BA.OrderScraper.Models.Syspro;
using Microsoft.EntityFrameworkCore;
using System.Configuration;

namespace BA.OrderScraper.EFCore
{
    public class SysproDbContext : DbContext
    {
        public DbSet<InvMaster> InvMaster { get; set; }
        public DbSet<SorMaster> SorMaster { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var connectionString = ConfigurationManager.ConnectionStrings["SysproDatabase"]?.ConnectionString;
            if (!string.IsNullOrEmpty(connectionString))
            {
                optionsBuilder.UseSqlServer(connectionString);
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<InvMaster>()
                .ToTable("InvMaster")
                .HasKey(e => e.StockCode);

            modelBuilder.Entity<SorMaster>()
                .ToTable("SorMaster")
                .HasKey(e => e.SalesOrder);
        }
    }
}