using BlogApp.Domain.Entities;

namespace BlogApp.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<Domain.Entities.User> Users { get; }

    DbSet<Post> Posts { get; }
    DbSet<Comment> Comments { get; }
    DbSet<RefreshToken> RefreshTokens { get; } 


    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
