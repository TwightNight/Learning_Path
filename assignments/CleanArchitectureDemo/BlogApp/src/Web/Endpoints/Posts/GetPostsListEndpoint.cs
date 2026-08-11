using BlogApp.Application.Common.Interfaces;
using BlogApp.Application.Posts.Queries.GetPostsList;
using FastEndpoints;

namespace BlogApp.Web.Endpoints.Posts;

public sealed class GetPostsListEndpoint : Endpoint<GetPostsRequest, List<GetPostsResponse>>
{
    private readonly IUser _currentUser;

    public GetPostsListEndpoint(IUser currentUser)
    {
        _currentUser = currentUser;
    }

    public override void Configure()
    {
        Get("/posts");
        AllowAnonymous(); // Trang blog công khai vẫn xem được, nhưng chỉ bài đã publish (xem handler)
    }

    public override async Task HandleAsync(GetPostsRequest req, CancellationToken ct)
    {
        int? requestorId = int.TryParse(_currentUser.Id, out var id) ? id : null;

        var command = new GetPostsCommand
        {
            AuthorId = req.AuthorId,
            RequestorId = requestorId
        };

        var response = await command.ExecuteAsync(ct);
        await Send.OkAsync(response, cancellation: ct);
    }
}