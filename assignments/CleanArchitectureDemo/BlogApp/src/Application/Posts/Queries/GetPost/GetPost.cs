using BlogApp.Application.Common.Interfaces;
using FastEndpoints;

namespace BlogApp.Application.Posts.Queries.GetPost;

public sealed class GetPostCommand : ICommand<GetPostResponse>
{
    public int Id { get; set; }
    public int? RequestorId { get; set; } // null nếu là khách vãng lai (anonymous)
}

public sealed class GetPostCommandHandler : ICommandHandler<GetPostCommand, GetPostResponse?>
{
    private readonly IApplicationDbContext _context;

    public GetPostCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<GetPostResponse?> ExecuteAsync(GetPostCommand command, CancellationToken ct)
    {
        var response = await _context.Posts
            .AsNoTracking()
            .Where(p => p.Id == command.Id)
            .Select(p => new GetPostResponse
            {
                Id = p.Id,
                Title = p.Title,
                Content = p.Content,
                PublishedDate = p.PublishedDate,
                IsPublished = p.IsPublished,
                Author = new AuthorDto { Id = p.Author.Id, FullName = p.Author.FullName },
                Comments = p.Comments.Select(c => new CommentDto
                {
                    Id = c.Id,
                    Content = c.Content,
                    UserId = c.UserId,
                    UserFullName = c.User.FullName
                }).ToList()
            })
            .FirstOrDefaultAsync(ct);

        if (response is null) return null;

        // Bài chưa publish chỉ chính tác giả mới xem được. Với người khác (kể cả đã login),
        // trả về null để endpoint báo 404 -> không tiết lộ là bài viết này có tồn tại.
        var isOwner = command.RequestorId is not null && response.Author?.Id == command.RequestorId;
        if (!response.IsPublished && !isOwner)
        {
            return null;
        }

        return response;
    }
}