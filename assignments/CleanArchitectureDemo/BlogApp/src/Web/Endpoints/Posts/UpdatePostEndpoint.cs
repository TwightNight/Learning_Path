using BlogApp.Application.Common.Interfaces;
using BlogApp.Application.Posts.Commands.UpdatePost;
using FastEndpoints;

namespace BlogApp.Web.Endpoints.Posts;

public sealed class UpdatePostEndpoint : Endpoint<UpdatePostRequest, UpdatePostResponse, UpdatePostMapper>
{
    private readonly IUser _currentUser;

    public UpdatePostEndpoint(IUser currentUser)
    {
        _currentUser = currentUser;
    }

    public override void Configure()
    {
        Put("/posts/{Id}");
        Roles("Author");
    }

    public override async Task<UpdatePostResponse> HandleAsync(UpdatePostRequest req, CancellationToken ct)
    {
        if (!int.TryParse(_currentUser.Id, out int requestorId))
        {
            ThrowError("The user's identity could not be determined.", 401);
        }

        var command = new UpdatePostCommand
        {
            Id = req.Id,
            RequestorId = requestorId,
            Title = req.Title,
            Content = req.Content
        };

        var result = await command.ExecuteAsync(ct);
        return Map.FromEntity(result);
    }
}