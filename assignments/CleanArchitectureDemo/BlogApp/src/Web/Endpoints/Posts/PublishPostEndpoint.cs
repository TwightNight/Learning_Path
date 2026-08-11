using BlogApp.Application.Common.Interfaces;
using BlogApp.Application.Posts.Commands.PublishPost;
using FastEndpoints;

namespace BlogApp.Web.Endpoints.Posts;

public sealed class PublishPostEndpoint : Endpoint<PublishPostRequest>
{
    private readonly IUser _currentUser;

    public PublishPostEndpoint(IUser currentUser)
    {
        _currentUser = currentUser;
    }

    public override void Configure()
    {
        Post("/posts/publish");
        Roles("Author");
    }

    public override async Task HandleAsync(PublishPostRequest req, CancellationToken ct)
    {
        if (!int.TryParse(_currentUser.Id, out int requestorId))
        {
            ThrowError("The user's identity could not be determined.", 401);
        }

        await new PublishPostCommand { Id = req.Id, RequestorId = requestorId }.ExecuteAsync(ct);
        await Send.NoContentAsync(ct);
    }
}