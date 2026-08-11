using BlogApp.Application.User.GetUsers;
using FastEndpoints;

namespace BlogApp.Web.Endpoints.Users;

public sealed class GetUsersEndpoint : EndpointWithoutRequest<List<GetUsersResponse>>
{
    public override void Configure()
    {
        Get("/users");
        Roles("Admin");
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var command = new GetUsersCommand();
        var response = await command.ExecuteAsync(ct);
        await Send.OkAsync(response, ct);
    }
}
