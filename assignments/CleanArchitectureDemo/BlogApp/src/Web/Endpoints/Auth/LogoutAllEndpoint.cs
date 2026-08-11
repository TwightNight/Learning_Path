using System.Security.Claims;
using BlogApp.Application.Auth.Logout;
using BlogApp.Application.Auth.LogoutAll;
using BlogApp.Application.Common.Interfaces;
using FastEndpoints;

namespace BlogApp.Web.Endpoints.Auth;

public sealed class LogoutAllEndpoint : EndpointWithoutRequest
{
    private readonly IUser _user;

    public LogoutAllEndpoint(IUser user)
    {
        _user = user;
    }
    public override void Configure()
    {
        Post("/auth/logout-all");
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        // var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!int.TryParse(_user.Id, out var userId))
        {
            throw new UnauthorizedAccessException("Invalid or missing user identifier claim.");
        }

        var command = new LogoutAllCommand { UserId = userId };
        await command.ExecuteAsync(ct);

        await Send.NoContentAsync(ct);
    }
}