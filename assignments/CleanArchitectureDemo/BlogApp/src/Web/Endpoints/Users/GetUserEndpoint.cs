using BlogApp.Application.Users.GetUser;
using FastEndpoints;

namespace BlogApp.Web.Endpoints.Users;

public sealed class GetUserEndpoint: Endpoint<GetUserRequest, GetUserResponse>
{
    public override void Configure()
    {
        Get("/users/{id}");
    }

    public override async Task HandleAsync(GetUserRequest req, CancellationToken ct)
    {
        var command = new GetUserCommand {Id = req.Id};
        var response = await command.ExecuteAsync(ct);

        await Send.OkAsync(response, ct);
    }
}