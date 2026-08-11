using BlogApp.Application.Auth.Login;
using BlogApp.Application.Common.Interfaces;
using FastEndpoints;
//
namespace BlogApp.Application.Auth.RefreshToken;

public sealed class RefreshTokenCommand : ICommand<LoginResponse>
{
    public string RefreshToken { get; set; } = string.Empty;
}

public sealed class RefreshTokenCommandHandler : ICommandHandler<RefreshTokenCommand, LoginResponse>
{
    private readonly IApplicationDbContext _context;
    private readonly IJwtTokenGenerator _tokenService;

    public RefreshTokenCommandHandler(IApplicationDbContext context, IJwtTokenGenerator tokenService)
    {
        _context = context;
        _tokenService = tokenService;
    }

    public async Task<LoginResponse> ExecuteAsync(RefreshTokenCommand command, CancellationToken ct)
    {
        var incomingHash = _tokenService.HashToken(command.RefreshToken);

        var existingToken = await _context.RefreshTokens
            .Include(r => r.User)
            .FirstOrDefaultAsync(r => r.TokenHash == incomingHash, ct);

        if (existingToken is null)
            throw new UnauthorizedAccessException("Invalid refresh token.");

        // REUSE DETECTION: nếu token này đã bị revoke mà vẫn được dùng lại
        // -> dấu hiệu bị đánh cắp -> revoke TOÀN BỘ token của user để an toàn
        if (existingToken.IsRevoked)
        {
            var allActiveTokens = await _context.RefreshTokens
                .Where(r => r.UserId == existingToken.UserId && r.RevokedAt == null)
                .ToListAsync(ct);

            foreach (var t in allActiveTokens)
                t.RevokedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(ct);
            throw new UnauthorizedAccessException("Refresh token reuse detected. All sessions revoked.");
        }

        if (existingToken.IsExpired)
            throw new UnauthorizedAccessException("Refresh token expired.");

        var user = existingToken.User;

        // ROTATION: revoke token cũ, phát hành token mới
        var (newAccessToken, accessExpiry) = _tokenService.GenerateAccessToken(user);
        var (newRawRefresh, newRefreshHash, newRefreshExpiry) = _tokenService.GenerateRefreshToken();

        var newRefreshEntity = new Domain.Entities.RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            TokenHash = newRefreshHash,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = newRefreshExpiry
        };

        existingToken.RevokedAt = DateTime.UtcNow;
        existingToken.ReplacedByTokenId = newRefreshEntity.Id;

        _context.RefreshTokens.Add(newRefreshEntity);
        await _context.SaveChangesAsync(ct);

        return new LoginResponse
        {
            AccessToken = newAccessToken,
            AccessTokenExpiry = accessExpiry,
            RefreshToken = newRawRefresh,
            RefreshTokenExpiry = newRefreshExpiry,
            UserId = user.Id,
            Role = user.Role
        };
    }
}