using BlogApp.Application.Common.Interfaces;
using BlogApp.Application.Posts.Commands.CreatePost;
using FastEndpoints;

namespace BlogApp.Web.Endpoints.Posts;

public sealed class CreatePostEndpoint: Endpoint<CreatePostRequest, CreatePostResponse, CreatePostMapper>
{

    private readonly IUser _currentUser;
    public CreatePostEndpoint(IUser currentUser)
    {
        _currentUser = currentUser;
    }

    public override void Configure()
    {
        Post("/posts");
        // AllowAnonymous();
        Roles("Author");
    }

    public override async Task<CreatePostResponse> HandleAsync(CreatePostRequest req, CancellationToken ct)
    {
        if (!int.TryParse(_currentUser.Id, out int authorId))
        {
            ThrowError("The user's identity could not be determined.", 401);
        }

        var command = new CreatePostCommand
        {
            Title = req.Title,
            Content = req.Content,
            AuthorId = authorId
        };

        var result = await command.ExecuteAsync(ct);
        var response = Map.FromEntity(result);
        return response;
    }
}