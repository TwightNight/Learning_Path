// Web/Endpoints/Users/ActivateUserEndpoint.cs
using BlogApp.Application.Users.Commands.ActivateUser;
using FastEndpoints;

namespace BlogApp.Web.Endpoints.Users;

public sealed class ActivateUserEndpoint : Endpoint<ActivateUserRequest>
{
    public override void Configure()
    {
        Post("/users/activate");
        Roles("Admin");
    }

    public override async Task HandleAsync(ActivateUserRequest req, CancellationToken ct)
    {
        var command = new ActivateUserCommand { TargetUserId = req.Id };
        await command.ExecuteAsync(ct);
        await Send.NoContentAsync(ct);
    }
}