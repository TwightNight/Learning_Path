using FastEndpoints;
using BlogApp.Application.Common.Interfaces;
using BlogApp.Application.Auth.Regis;

namespace BlogApp.Web.Auth.Regis;

public class RegisEndpoint : Endpoint<RegisRequest, RegisResponse, RegisMapper>
{

    public override void Configure()
    {
        Post("/auth/register");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Register";
            s.Description = "Create a new user account";
        });
    }

    public override async Task HandleAsync(RegisRequest req, CancellationToken ct)
    {
        var command = new RegisCommand
        {
            FirstName = req.FirstName,
            LastName = req.LastName,
            UserName = req.UserName,
            Email = req.Email,
            Password = req.Password
        };


        var user = await command.ExecuteAsync(ct);

        var response = Map.FromEntity(user);

        await Send.OkAsync(response, ct);
    }

}
