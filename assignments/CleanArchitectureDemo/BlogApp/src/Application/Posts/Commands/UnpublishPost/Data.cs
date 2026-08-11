// UnpublishPostCommand.cs
using BlogApp.Application.Common.Exceptions;
using BlogApp.Application.Common.Interfaces;
using BlogApp.Domain.Entities;
using FastEndpoints;

namespace BlogApp.Application.Posts.Commands.UnpublishPost;

public sealed class UnpublishPostCommand : ICommand
{
    public int Id { get; set; }
    public int RequestorId { get; set; }
}

public sealed class UnpublishPostCommandHandler : ICommandHandler<UnpublishPostCommand>
{
    private readonly IApplicationDbContext _context;

    public UnpublishPostCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task ExecuteAsync(UnpublishPostCommand command, CancellationToken ct)
    {
        var post = await _context.Posts.FirstOrDefaultAsync(p => p.Id == command.Id, ct);

        if (post is null)
        {
            throw new NotFoundException(nameof(Post), $"{command.Id}");
        }

        if (post.AuthorId != command.RequestorId)
        {
            throw new ForbiddenAccessException("You are not allowed to unpublish this post.");
        }

        post.IsPublished = false;
        // Không xoá PublishedDate -> giữ lại lịch sử lần publish gần nhất.

        await _context.SaveChangesAsync(ct);
    }
}