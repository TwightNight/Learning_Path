// Web/Endpoints/Posts/UnpublishPostEndpoint.cs
using BlogApp.Application.Common.Interfaces;
using BlogApp.Application.Posts.Commands.UnpublishPost;
using FastEndpoints;

namespace BlogApp.Web.Endpoints.Posts;

public sealed class UnpublishPostEndpoint : Endpoint<UnpublishPostRequest>
{
    private readonly IUser _currentUser;

    public UnpublishPostEndpoint(IUser currentUser)
    {
        _currentUser = currentUser;
    }

    public override void Configure()
    {
        Post("/posts/unpublish");
        Roles("Author");
    }

    public override async Task HandleAsync(UnpublishPostRequest req, CancellationToken ct)
    {
        if (!int.TryParse(_currentUser.Id, out int requestorId))
        {
            ThrowError("The user's identity could not be determined.", 401);
        }

        await new UnpublishPostCommand { Id = req.Id, RequestorId = requestorId }.ExecuteAsync(ct);
        await Send.NoContentAsync(ct);
    }
}