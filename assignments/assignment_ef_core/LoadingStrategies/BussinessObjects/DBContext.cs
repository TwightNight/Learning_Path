using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Reflection.PortableExecutable;
using System.Text;

namespace BussinessObject
{
    public class DBContext : DbContext
    {
        public DbSet<Product> Products {  get; set; }
        public DbSet<Category> Categories { get; set; }

        public string DbPath;

        public DBContext()
        {
            //DbPath = Path.Combine(Directory.GetCurrentDirectory(), "product.db");
            DbPath = Path.GetFullPath(
                Path.Combine(
                    AppContext.BaseDirectory,
                    @"..\..\..\..\BussinessObjects\product.db"));
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder
            .UseLazyLoadingProxies()
            .UseSqlite($"Data Source={DbPath}");

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Category>().HasData(
                new Category { Id = 1, Name = "Electronics" },
                new Category { Id = 2, Name = "Food" }
            );

            modelBuilder.Entity<Product>().HasData(
                new Product { Id = 1, Name = "Laptop", CategoryId = 1 },
                new Product { Id = 2, Name = "Apple", CategoryId = 2 }
            );
        }
    }
}
