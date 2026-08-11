using BlogApp.Application.Common.Interfaces;
using BlogApp.Domain.Entities;
using FastEndpoints;

namespace BlogApp.Application.Posts.Commands.CreatePost;

public sealed class CreatePostCommand: ICommand<Post>
{
    public int AuthorId { get; set; }
    public required string Title { get; set; }
    public required string Content { get; set; }
}

public sealed class CreatePostCommandHandler: ICommandHandler<CreatePostCommand, Post>
{
    private readonly IApplicationDbContext _context;

    public CreatePostCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }
    public async Task<Post> ExecuteAsync(CreatePostCommand command, CancellationToken ct)
    {
        var author = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == command.AuthorId, ct);

        if (author is null)
        {
            throw new NotFoundException(nameof(User), $"{command.AuthorId}");
        }

        var post = new Post
        {
            AuthorId = command.AuthorId,
            Title = command.Title,
            Content = command.Content,
            IsPublished = false,
            Created = DateTime.UtcNow,
        };

        _context.Posts.Add(post);
        await _context.SaveChangesAsync(ct);
        
        return post;
    }
}