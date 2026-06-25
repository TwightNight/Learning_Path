using Microsoft.EntityFrameworkCore;
using NovelManangment.Models;

namespace NovelManangment.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Genre> Genres { get; set; }
        public DbSet<Novel> Novels { get; set; }
        public DbSet<Volume> Volumes { get; set; }
        public DbSet<Chapter> Chapters { get; set; }
        public DbSet<NovelFollow> Follows { get; set; }
        public DbSet<NovelRating> NovelRatings { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<SiteAlert> SiteAlerts { get; set; }
        public DbSet<NovelReview> NovelReviews { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(
                typeof(ApplicationDbContext).Assembly);

            base.OnModelCreating(modelBuilder);
        }
    }
}
