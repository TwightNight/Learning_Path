using BlogApp.Application.Common.Interfaces;
using FastEndpoints;

namespace BlogApp.Application.Auth.Logout;

public sealed class LogoutCommand : ICommand
{
    public string RefreshToken { get; set; } = string.Empty;
}

public sealed class LogoutCommandHandler : ICommandHandler<LogoutCommand>
{
    private readonly IApplicationDbContext _applicationDb;
    private readonly IJwtTokenGenerator _tokenService;

    public LogoutCommandHandler(IApplicationDbContext applicationDb, IJwtTokenGenerator tokenService)
    {
        _applicationDb = applicationDb;
        _tokenService = tokenService;
    }

    public async Task ExecuteAsync(LogoutCommand command, CancellationToken ct)
    {
        var hash = _tokenService.HashToken(command.RefreshToken);

        var token = await _applicationDb.RefreshTokens
            .FirstOrDefaultAsync(r => r.TokenHash == hash, ct);

        if (token is not null && token.RevokedAt is null)
        {
            token.RevokedAt = DateTime.UtcNow;
            await _applicationDb.SaveChangesAsync(ct);
        }
    }
}