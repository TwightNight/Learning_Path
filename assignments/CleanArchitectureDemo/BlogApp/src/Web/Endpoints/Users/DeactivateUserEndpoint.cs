// Web/Endpoints/Users/DeactivateUserEndpoint.cs
using BlogApp.Application.Common.Interfaces;
using BlogApp.Application.Users.Commands.DeactivateUser;
using FastEndpoints;

namespace BlogApp.Web.Endpoints.Users;

public sealed class DeactivateUserEndpoint : Endpoint<DeactivateUserRequest>
{
    private readonly IUser _currentUser;

    public DeactivateUserEndpoint(IUser currentUser)
    {
        _currentUser = currentUser;
    }

    public override void Configure()
    {
        Post("/users/deactivate");
        Roles("Admin");
        Summary(s =>
        {
            s.Summary = "Deactivate user";
            s.Description = "Deactivates a user account and immediately revokes their active sessions.";
        });
    }

    public override async Task HandleAsync(DeactivateUserRequest req, CancellationToken ct)
    {
        if (!int.TryParse(_currentUser.Id, out int requestorId))
        {
            ThrowError("The user's identity could not be determined.", 401);
        }

        var command = new DeactivateUserCommand
        {
            TargetUserId = req.Id,
            RequestorId = requestorId
        };

        await command.ExecuteAsync(ct);
        await Send.NoContentAsync(ct);
    }
}