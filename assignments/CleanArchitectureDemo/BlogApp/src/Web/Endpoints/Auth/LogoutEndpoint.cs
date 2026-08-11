using BlogApp.Application.Auth.Logout;
using BlogApp.Application.Common.Exceptions;
using FastEndpoints;

namespace BlogApp.Web.Endpoints.Auth;

public sealed class LogoutEndpoint : Endpoint<LogoutRequest>
{
    public override void Configure()
    {
        Post("/auth/logout");
        AllowAnonymous();
    }

    public override async Task HandleAsync(LogoutRequest req, CancellationToken ct)
    {
        var command = new LogoutCommand { RefreshToken = req.RefreshToken };
        await command.ExecuteAsync(ct);

        await Send.NoContentAsync(ct);
    }
}