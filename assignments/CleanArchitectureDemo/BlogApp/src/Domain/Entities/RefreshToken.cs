// BlogApp.Domain/Entities/RefreshToken.cs
namespace BlogApp.Domain.Entities;

public class RefreshToken
{
    public Guid Id { get; set; }
    public int UserId { get; set; }

    public string TokenHash { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public DateTime? RevokedAt { get; set; }

    // Dùng cho rotation chain + reuse detection
    public Guid? ReplacedByTokenId { get; set; }

    public string? CreatedByIp { get; set; }

    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
    public bool IsRevoked => RevokedAt is not null;
    public bool IsActive => !IsRevoked && !IsExpired;

    public User User { get; set; } = null!;
}