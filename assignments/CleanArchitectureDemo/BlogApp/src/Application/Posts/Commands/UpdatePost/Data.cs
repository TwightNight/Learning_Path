using BlogApp.Application.Common.Exceptions;
using BlogApp.Application.Common.Interfaces;
using BlogApp.Domain.Entities;
using FastEndpoints;

namespace BlogApp.Application.Posts.Commands.UpdatePost;

public sealed class UpdatePostCommand : ICommand<Post>
{
    public int Id { get; set; }
    public int RequestorId { get; set; } // Người thực hiện request, dùng để check ownership
    public required string Title { get; set; }
    public required string Content { get; set; }
}

public sealed class UpdatePostCommandHandler : ICommandHandler<UpdatePostCommand, Post>
{
    private readonly IApplicationDbContext _context;

    public UpdatePostCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Post> ExecuteAsync(UpdatePostCommand command, CancellationToken ct)
    {
        // Không AsNoTracking() vì cần EF track entity để SaveChangesAsync phát hiện thay đổi
        var post = await _context.Posts
            .FirstOrDefaultAsync(p => p.Id == command.Id, ct);

        if (post is null)
        {
            throw new NotFoundException(nameof(Post), $"{command.Id}");
        }

        if (post.AuthorId != command.RequestorId)
        {
            throw new ForbiddenAccessException("You are not allowed to modify this post.");
        }

        post.Title = command.Title;
        post.Content = command.Content;
        post.LastModified = DateTime.UtcNow;

        await _context.SaveChangesAsync(ct);

        return post;
    }
}