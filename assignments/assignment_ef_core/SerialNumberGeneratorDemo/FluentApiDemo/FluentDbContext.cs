using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace FluentApiDemo
{
    public class FluentDbContext : DbContext
    {
        public DbSet<ProductFluent> Products { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(@"Server=(localdb)\mssqllocaldb;Database=SerialDemo_Fluent_DB;Trusted_Connection=True;");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            
            modelBuilder.Entity<ProductFluent>()
                .Property(p => p.CreatedDate)
                .HasDefaultValueSql("GETDATE()");

            // Declare the formula to calculate the Serial Number directly in the C# code.
            modelBuilder.Entity<ProductFluent>()
                .Property(p => p.SerialNumber)
                .HasComputedColumnSql(
                    "CAST([Id] AS VARCHAR) + '-' + CONVERT(VARCHAR(8), [CreatedDate], 112) + '-' + CAST([Id] AS VARCHAR)",
                    stored: true);
        }
    }
}
