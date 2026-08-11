using BlogApp.Application.Common.Interfaces;
using FastEndpoints;

namespace BlogApp.Application.Posts.Queries.GetPostsList;

public sealed class GetPostsCommand : ICommand<List<GetPostsResponse>>
{
    public int? AuthorId { get; set; }
    public int? RequestorId { get; set; } // Người đang gọi API (null nếu anonymous)
    public bool IncludeAllStatuses { get; set; } // true chỉ dùng cho endpoint Admin
}

public sealed class GetPostsCommandHandler : ICommandHandler<GetPostsCommand, List<GetPostsResponse>>
{
    private readonly IApplicationDbContext _context;

    public GetPostsCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<GetPostsResponse>> ExecuteAsync(GetPostsCommand command, CancellationToken ct)
    {
        var query = _context.Posts.AsNoTracking().AsQueryable();

        if (command.AuthorId is not null)
        {
            query = query.Where(p => p.AuthorId == command.AuthorId);
        }

        if (!command.IncludeAllStatuses)
        {
            // Chỉ chính tác giả mới thấy bài nháp (IsPublished = false) của mình;
            // khách vãng lai hoặc xem trang tác giả khác chỉ thấy bài đã xuất bản.
            var isOwner = command.AuthorId is not null && command.AuthorId == command.RequestorId;
            if (!isOwner)
            {
                query = query.Where(p => p.IsPublished);
            }
        }

        return await query
            .OrderByDescending(p => p.LastModified)
            .Select(p => new GetPostsResponse
            {
                Id = p.Id,
                Title = p.Title,
                Content = p.Content,
                PublishedDate = p.PublishedDate,
                IsPublished = p.IsPublished,
                AuthorId = p.AuthorId,
                AuthorFullName = p.Author.FullName,
                CommentsCount = p.Comments.Count
            })
            .ToListAsync(ct);
    }
}