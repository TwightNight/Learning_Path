using BlogApp.Application.Common.Exceptions;
using BlogApp.Application.Common.Interfaces;
using BlogApp.Domain.Entities;
using FastEndpoints;

namespace BlogApp.Application.Users.Commands.DeactivateUser;

public sealed class DeactivateUserCommand : ICommand
{
    public int TargetUserId { get; set; }
    public int RequestorId { get; set; } // Admin thực hiện hành động, dùng để chặn tự khoá chính mình
}

public sealed class DeactivateUserCommandHandler : ICommandHandler<DeactivateUserCommand>
{
    private readonly IApplicationDbContext _context;

    public DeactivateUserCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task ExecuteAsync(DeactivateUserCommand command, CancellationToken ct)
    {
        if (command.TargetUserId == command.RequestorId)
        {
            throw new ForbiddenAccessException("You cannot deactivate your own account.");
        }

        var user = await _context.Users
            .Include(u => u.RefreshTokens)
            .FirstOrDefaultAsync(u => u.Id == command.TargetUserId, ct);

        if (user is null)
        {
            throw new NotFoundException(nameof(User), $"{command.TargetUserId}");
        }

        if (!user.IsActive)
        {
            return; // Idempotent: đã inactive thì không làm gì thêm, tránh lỗi khi Admin double-click
        }

        user.IsActive = false;

        // Tăng TokenVersioning -> mọi access token cũ (đã ký với version cũ) sẽ bị coi là invalid
        // ở middleware validate token, dù chưa hết hạn theo thời gian.
        user.TokenVersioning += 1;

        // Revoke toàn bộ refresh token còn sống để chặn luôn đường refresh lại access token mới.
        foreach (var token in user.RefreshTokens.Where(t => t.RevokedAt == null))
        {
            token.RevokedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync(ct);
    }
}