using BlogApp.Application.Common.Interfaces;
using BlogApp.Application.Posts.Queries.GetPost;
using BlogApp.Domain.Entities;
using FastEndpoints;

namespace BlogApp.Web.Endpoints.Posts;

public sealed class GetPostEndpoint : Endpoint<GetPostRequest>
{
    private readonly IUser _currentUser;

    public GetPostEndpoint(IUser currentUser)
    {
        _currentUser = currentUser;
    }

    public override void Configure()
    {
        Get("/posts/{id:int}");
        AllowAnonymous(); // Khách vãng lai được xem bài đã publish; handler tự chặn bài draft
    }

    public override async Task HandleAsync(GetPostRequest req, CancellationToken ct)
    {
        int? requestorId = int.TryParse(_currentUser.Id, out var id) ? id : null;

        var command = new GetPostCommand { Id = req.Id, RequestorId = requestorId };
        var response = await command.ExecuteAsync(ct);

        if (response is null)
        {
            throw new NotFoundException(nameof(Post), $"{req.Id}");
        }

        await Send.OkAsync(response, cancellation: ct);
    }
}