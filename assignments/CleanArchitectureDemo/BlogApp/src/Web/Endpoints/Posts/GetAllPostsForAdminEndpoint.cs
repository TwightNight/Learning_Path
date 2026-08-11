using BlogApp.Application.Posts.Queries.GetPostsList;
using FastEndpoints;

namespace BlogApp.Web.Endpoints.Posts;

public sealed class GetAllPostsForAdminEndpoint : EndpointWithoutRequest<List<GetPostsResponse>>
{
    public override void Configure()
    {
        Get("/admin/posts");
        Roles("Admin"); // Thấy toàn bộ bài viết kể cả nháp của mọi tác giả
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var command = new GetPostsCommand { IncludeAllStatuses = true };
        var response = await command.ExecuteAsync(ct);
        await Send.OkAsync(response, ct);
    }
}