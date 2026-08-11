using BlogApp.Application.Common.Interfaces;
using FastEndpoints;

namespace BlogApp.Application.Auth.LogoutAll;

public sealed class LogoutAllCommand : ICommand
{
    public int UserId { get; set; } 
}

public sealed class LogoutAllCommandHandler : ICommandHandler<LogoutAllCommand>
{
    private readonly IApplicationDbContext _applicationDb;

    public LogoutAllCommandHandler(IApplicationDbContext applicationDb)
    {
        _applicationDb = applicationDb;
    }

    public async Task ExecuteAsync(LogoutAllCommand command, CancellationToken ct)
    {
        var user = await _applicationDb.Users
            .FirstOrDefaultAsync(u => u.Id == command.UserId, ct);

        if (user is null) return;

        user.TokenVersioning++;

        var activeTokens = await _applicationDb.RefreshTokens
            .Where(r => r.UserId == command.UserId && r.RevokedAt == null)
            .ToListAsync(ct);

        foreach (var t in activeTokens)
            t.RevokedAt = DateTime.UtcNow;

        await _applicationDb.SaveChangesAsync(ct);
    }
}