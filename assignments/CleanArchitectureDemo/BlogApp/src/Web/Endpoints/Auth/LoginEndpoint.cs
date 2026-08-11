using BlogApp.Application.Auth.Login;
using FastEndpoints;

namespace BlogApp.Web.Endpoints.Auth;

public class LoginEndpoint : Endpoint<LoginRequest, LoginResponse>
{
    public override void Configure()
    {
        Post("/auth/login");
        AllowAnonymous(); // Cho phép truy cập công khai không cần Token
        Summary(s =>
        {
            s.Summary = "Login";
            s.Description = "Authenticate Email/Password and return JWT Token";
        });
    }

    public override async Task HandleAsync(LoginRequest req, CancellationToken ct)
    {
        
        var command = new LoginCommand { Email = req.Email, Password = req.Password };
        var result = await command.ExecuteAsync(ct);
        await Send.OkAsync(result, ct);
    }
}