using BlogApp.Application.Common.Exceptions;
using BlogApp.Application.Common.Interfaces;
using BlogApp.Domain.Entities;
using FastEndpoints;

namespace BlogApp.Application.Users.Commands.ActivateUser;

public sealed class ActivateUserCommand : ICommand
{
    public int TargetUserId { get; set; }
}

public sealed class ActivateUserCommandHandler : ICommandHandler<ActivateUserCommand>
{
    private readonly IApplicationDbContext _context;

    public ActivateUserCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task ExecuteAsync(ActivateUserCommand command, CancellationToken ct)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == command.TargetUserId, ct);

        if (user is null)
        {
            throw new NotFoundException(nameof(User), $"{command.TargetUserId}");
        }

        if (user.IsActive)
        {
            return; // Idempotent
        }

        user.IsActive = true;
        // Không cần đổi TokenVersioning khi activate lại — user activate xong vẫn phải login lại
        // từ đầu vì refresh token cũ đã bị revoke ở bước deactivate, đây là hành vi đúng (không auto-login lại).

        await _context.SaveChangesAsync(ct);
    }
}