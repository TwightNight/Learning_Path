using BlogApp.Application.Common.Exceptions;
using BlogApp.Application.Common.Interfaces;
using BlogApp.Domain.Entities;
using FastEndpoints;

namespace BlogApp.Application.Posts.Commands.DeletePost;

// ICommand không generic (không có kết quả trả về) - FastEndpoints hỗ trợ ICommandHandler<TCommand>
public sealed class DeletePostCommand : ICommand
{
    public int Id { get; set; }
    public int RequestorId { get; set; }
}

public sealed class DeletePostCommandHandler : ICommandHandler<DeletePostCommand>
{
    private readonly IApplicationDbContext _context;

    public DeletePostCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task ExecuteAsync(DeletePostCommand command, CancellationToken ct)
    {
        var post = await _context.Posts
            .FirstOrDefaultAsync(p => p.Id == command.Id, ct);

        if (post is null)
        {
            throw new NotFoundException(nameof(Post), $"{command.Id}");
        }

        if (post.AuthorId != command.RequestorId)
        {
            throw new ForbiddenAccessException("You are not allowed to delete this post.");
        }

        _context.Posts.Remove(post);
        await _context.SaveChangesAsync(ct);
    }
}