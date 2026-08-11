// BlogApp.Web/Endpoints/Auth/RefreshTokenEndpoint.cs
using BlogApp.Application.Auth.Login;
using BlogApp.Application.Auth.RefreshToken;
using FastEndpoints;

namespace BlogApp.Web.Endpoints.Auth;

public sealed class RefreshTokenEndpoint : Endpoint<RefreshTokenRequest, LoginResponse>
{
    public override void Configure()
    {
        Post("/auth/refresh");
        AllowAnonymous();
    }

    public override async Task HandleAsync(RefreshTokenRequest req, CancellationToken ct)
    {
        var command = new RefreshTokenCommand { RefreshToken = req.RefreshToken };
        var result = await command.ExecuteAsync(ct);
        await Send.OkAsync(result, cancellation: ct);
    }
}