using FastEndpoints;
using BlogApp.Domain.Entities;
using BlogApp.Application.Common.Interfaces;

namespace BlogApp.Application.Auth.Regis;

public sealed class RegisCommand: ICommand<Domain.Entities.User>
{

    public string FirstName { get; set; } = default!;
    public string LastName { get; set; } = default!;
    public string UserName { get; set; } = default!;
    public string Email { get; set; } = default!;
    public string Password { get; set; } = default!;
}

public sealed class RegisCommandHandler: ICommandHandler<RegisCommand, Domain.Entities.User>
{
    private readonly IApplicationDbContext _context;
    public RegisCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }
    public async Task<Domain.Entities.User> ExecuteAsync(RegisCommand command, CancellationToken ct)
    {
        var alreadyUser = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Email == command.Email);

        if (alreadyUser is not null)
        {
            throw new ValidationException("The user have already existed");
        }

        var newUser = new Domain.Entities.User
        {
            FirstName = command.FirstName,
            LastName = command.LastName,
            UserName = command.UserName,
            Email = command.Email,
            PasswordHash = command.Password, 
            Role = "User"
        };

        _context.Users.Add(newUser);
        await _context.SaveChangesAsync(ct);

        return newUser;
    }
}