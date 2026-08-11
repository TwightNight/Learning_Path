using BlogApp.Domain.Entities;

namespace BlogApp.Application.Common.Interfaces;

public interface IJwtTokenGenerator
{
    (string AccessToken, DateTime AccessExpiry) GenerateAccessToken(Domain.Entities.User user);
    (string RawToken, string TokenHash, DateTime Expiry) GenerateRefreshToken();
    string HashToken(string rawToken);
}