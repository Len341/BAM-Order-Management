using BA.OrderScraper.Models;
using Microsoft.EntityFrameworkCore;
using System.Configuration;

namespace BA.OrderScraper.EFCore
{
    public class BADbContext : DbContext
    {
        public DbSet<SysproOrderCreationHistory> SysproOrderCreationHistory { get; set; }
        public DbSet<Error> Error { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"]?.ConnectionString;
            if (!string.IsNullOrEmpty(connectionString))
            {
                optionsBuilder.UseSqlServer(connectionString);
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<SysproOrderCreationHistory>()
                .HasKey(e => e.Id);
                
            modelBuilder.Entity<Error>()
                .HasKey(e => e.Id);
        }
    }
}