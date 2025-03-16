using BA.OrderScraper.Models;
using Microsoft.EntityFrameworkCore;
using System.Configuration;
//using Microsoft.Extensions.Configuration;

namespace BA.OrderScraper.EFCore
{
    public class BADbContext : DbContext
    {
        public DbSet<KanbanCard> StagingKanbanCard { get; set; }
        public DbSet<Manifest> StagingManifest { get; set; }
        public DbSet<Skid> StagingSkid { get; set; }
        public DbSet<SysproOrderCreationHistory> SysproOrderCreationHistory { get; set; }
        public DbSet<Error> Error { get; set; }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer($@"Server={ConfigurationManager.AppSettings["DatabaseServerName"]};Database={ConfigurationManager.AppSettings["DatabaseName"]};Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Manifest>()
                .HasKey(z => new { z.SupplierManifestNo, z.SupplierKanbanNumber, z.SupplierPadEasyReferenceNumber });
        }
    }
}
