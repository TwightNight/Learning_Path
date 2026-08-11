using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using BlogApp.Application.Common.Interfaces;
using BlogApp.Domain.Entities;
using FastEndpoints.Security;
using Microsoft.Extensions.Configuration;

namespace BlogApp.Infrastructure.Auth;

public sealed class JwtTokenGenerator : IJwtTokenGenerator
{
    private readonly IConfiguration _configuration;
    private readonly int _accessTokenMinutes;
    private readonly int _refreshTokenDays;

    public JwtTokenGenerator(IConfiguration configuration)
    {
        _configuration = configuration;
        _accessTokenMinutes = configuration.GetValue("Jwt:AccessTokenExpiryMinutes", 15);
        _refreshTokenDays = configuration.GetValue("Jwt:RefreshTokenExpiryDays", 7);       
    }

public (string AccessToken, DateTime AccessExpiry) GenerateAccessToken(User user)
    {
        var signingKey = _configuration["Jwt:SigningKey"]
            ?? throw new InvalidOperationException("Jwt:SigningKey is not configured.");

        var expiry = DateTime.UtcNow.AddMinutes(_accessTokenMinutes);

        var token = JwtBearer.CreateToken(o =>
        {
            o.SigningKey = signingKey;
            o.ExpireAt = expiry;
            o.User.Roles.Add(user.Role);
            o.User.Claims.Add(new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()));
            o.User.Claims.Add(new Claim("tv", user.TokenVersioning.ToString()));
        });

        return (token, expiry);
    }

    public (string RawToken, string TokenHash, DateTime Expiry) GenerateRefreshToken()
    {
        var rawBytes = RandomNumberGenerator.GetBytes(64);
        var rawToken = Convert.ToBase64String(rawBytes);
        var expiry = DateTime.UtcNow.AddDays(_refreshTokenDays);

        return (rawToken, HashToken(rawToken), expiry);
    }

    public string HashToken(string rawToken)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(rawToken));
        return Convert.ToBase64String(bytes);
    }
}