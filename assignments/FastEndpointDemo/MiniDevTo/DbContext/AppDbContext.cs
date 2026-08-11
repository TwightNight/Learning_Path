using Microsoft.EntityFrameworkCore;
using MiniDevTo.Entities;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Author> Authors => Set<Author>();
    public DbSet<Article> Articles => Set<Article>();
}