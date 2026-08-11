using BlogApp.Application.Common.Interfaces;
using BlogApp.Application.Posts.Commands.DeletePost;
using FastEndpoints;

namespace BlogApp.Web.Endpoints.Posts;

public sealed class DeletePostEndpoint : Endpoint<DeletePostRequest>
{
    private readonly IUser _currentUser;

    public DeletePostEndpoint(IUser currentUser)
    {
        _currentUser = currentUser;
    }

    public override void Configure()
    {
        Delete("/posts/{Id}");
        Roles("Author");
    }

    public override async Task HandleAsync(DeletePostRequest req, CancellationToken ct)
    {
        if (!int.TryParse(_currentUser.Id, out int requestorId))
        {
            ThrowError("The user's identity could not be determined.", 401);
        }

        var command = new DeletePostCommand
        {
            Id = req.Id,
            RequestorId = requestorId
        };

        await command.ExecuteAsync(ct);
        await Send.NoContentAsync(ct);
    }
}