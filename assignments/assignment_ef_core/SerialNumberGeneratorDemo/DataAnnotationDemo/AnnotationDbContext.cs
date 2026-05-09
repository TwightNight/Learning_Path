using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataAnnotationDemo
{
    public class AnnotationDbContext : DbContext
    {
        public DbSet<ProductAnnotation> Products { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // Database riêng cho Annotation
            optionsBuilder.UseSqlServer(@"Server=(localdb)\mssqllocaldb;Database=SerialDemo_Annotation_DB;Trusted_Connection=True;");
        }
    }
}
