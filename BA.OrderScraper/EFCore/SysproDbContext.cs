using BA.OrderScraper.Models;
using BA.OrderScraper.Models.Syspro;
using Microsoft.EntityFrameworkCore;
using System.Configuration;
//using Microsoft.Extensions.Configuration;

namespace BA.OrderScraper.EFCore
{
    public class SysproDbContext : DbContext
    {
        public DbSet<InvMaster> InvMaster { get; set; }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer($@"Server={ConfigurationManager.AppSettings["SysproDatabaseServerName"]};Database={ConfigurationManager.AppSettings["SysproDatabaseName"]};Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //modelBuilder.Entity<Manifest>()
            //    .HasKey(z => new { z.SupplierManifestNo, z.SupplierKanbanNumber, z.SupplierPadEasyReferenceNumber });
        }
    }
}
