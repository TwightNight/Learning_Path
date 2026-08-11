using System.Security.Claims;
using System.Text;
using BlogApp.Application.Common.Interfaces;
using BlogApp.Domain.Entities;
using FastEndpoints;
using FastEndpoints.Security;
using Microsoft.Extensions.Configuration;

namespace BlogApp.Application.Auth.Login;

public sealed class LoginCommand : ICommand<LoginResponse>
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public sealed class LoginCommandHandler : ICommandHandler<LoginCommand, LoginResponse>
{
    private readonly IApplicationDbContext _context;
    // private readonly IConfiguration _configuration;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;


    public LoginCommandHandler(IApplicationDbContext context, IJwtTokenGenerator jwtTokenGenerator)
    {
        _context = context;
        _jwtTokenGenerator = jwtTokenGenerator;
    }
    public async Task<LoginResponse> ExecuteAsync(LoginCommand command, CancellationToken ct)
    {
        var user = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Email == command.Email, ct);
        if (user is null || user.PasswordHash != command.Password)
        {
            throw new UnauthorizedAccessException("Invalid email or password.");
        }
        if (!user.IsActive)
        {
            throw new UnauthorizedAccessException("This account has been deactivated.");
        }
        var (accessToken, accessExpiry) = _jwtTokenGenerator.GenerateAccessToken(user);
        var (rawRefreshToken, refreshHash, refreshExpiry) = _jwtTokenGenerator.GenerateRefreshToken();

        _context.RefreshTokens.Add(new Domain.Entities.RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            TokenHash = refreshHash,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = refreshExpiry
        });

        await _context.SaveChangesAsync(ct);

        return new LoginResponse
        {
            AccessToken = accessToken,
            AccessTokenExpiry = accessExpiry,
            RefreshToken = rawRefreshToken, // raw token trả về client, DB chỉ giữ hash
            RefreshTokenExpiry = refreshExpiry,
            UserId = user.Id,
            Role = user.Role
        };
    }

    // public string GenerateToken(Domain.Entities.User user, DateTime expiry)
    // {
    //     var signingKey = _configuration["Jwt:SigningKey"]
    //         ?? throw new InvalidOperationException("Jwt:SigningKey is not configured.");
    //     var jwtToken = JwtBearer.CreateToken(
    //         o =>
    //         {
    //             o.SigningKey = signingKey;
    //             o.ExpireAt = expiry;
    //             o.User.Roles.Add(user.Role);
    //             o.User.Claims.Add(new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()));
    //             o.User.Claims.Add(new Claim("tv", user.TokenVersioning.ToString()));
    //         }
    //     );
    //     return jwtToken;
    // }
}