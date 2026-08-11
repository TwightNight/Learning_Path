using BlogApp.Application.Common.Exceptions;
using BlogApp.Application.Common.Interfaces;
using BlogApp.Domain.Entities;
using FastEndpoints;

namespace BlogApp.Application.Posts.Commands.PublishPost;

public sealed class PublishPostCommand : ICommand
{
    public int Id { get; set; }
    public int RequestorId { get; set; }
}

public sealed class PublishPostCommandHandler : ICommandHandler<PublishPostCommand>
{
    private readonly IApplicationDbContext _context;

    public PublishPostCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task ExecuteAsync(PublishPostCommand command, CancellationToken ct)
    {
        var post = await _context.Posts.FirstOrDefaultAsync(p => p.Id == command.Id, ct);

        if (post is null)
        {
            throw new NotFoundException(nameof(Post), $"{command.Id}");
        }

        if (post.AuthorId != command.RequestorId)
        {
            throw new ForbiddenAccessException("You are not allowed to publish this post.");
        }

        post.IsPublished = true;
        // Giữ nguyên ngày publish gốc nếu đã từng publish trước đó (unpublish rồi publish lại
        // không nên coi như bài mới), chỉ set lần đầu tiên.
        post.PublishedDate ??= DateTime.UtcNow;

        await _context.SaveChangesAsync(ct);
    }
}