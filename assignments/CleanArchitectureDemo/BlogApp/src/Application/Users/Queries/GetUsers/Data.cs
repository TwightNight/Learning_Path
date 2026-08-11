using BlogApp.Application.Common.Interfaces;
using FastEndpoints;

namespace BlogApp.Application.User.GetUsers;

public sealed class GetUsersCommand: ICommand<List<GetUsersResponse>>
{
    
}

public sealed class GetUsersCommandHandler: ICommandHandler<GetUsersCommand, List<GetUsersResponse>>
{
    private readonly IApplicationDbContext _context;

    public GetUsersCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<GetUsersResponse>> ExecuteAsync(GetUsersCommand command, CancellationToken ct)
    {
        var user = await _context.Users
            .AsNoTracking()
            .Select(u => new GetUsersResponse
            {
                Id = u.Id,
                UserName = u.UserName,
                Email = u.Email,
                FullName = u.FullName,
                Role = u.Role,
                IsActive = u.IsActive
            })
            .ToListAsync();

        return user;
        
    }
}
