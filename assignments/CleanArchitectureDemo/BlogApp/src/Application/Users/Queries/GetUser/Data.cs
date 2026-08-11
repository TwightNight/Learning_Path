using BlogApp.Application.Common.Interfaces;
using FastEndpoints;

namespace BlogApp.Application.Users.GetUser;

public sealed class GetUserCommand: ICommand<GetUserResponse>
{
    public int Id {get; set;}
}

public sealed class GetUserCommandHandler: ICommandHandler<GetUserCommand, GetUserResponse>
{
    private readonly IApplicationDbContext _context;

    public GetUserCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }
    public async Task<GetUserResponse> ExecuteAsync(GetUserCommand command, CancellationToken ct)
    {
        var user = await _context.Users
            .AsNoTracking()
            .Where(u => u.Id == command.Id)
            .Select(u => new GetUserResponse
            {
                FirstName = u.FirstName,
                LastName = u.LastName,
                FullName = u.FullName,
                UserName = u.UserName,
                Email = u.Email
            })
            .FirstOrDefaultAsync();

        if (user is null)
        {
            throw new NotFoundException($"{command.Id}", "User");
        }

        return user;
    }
}