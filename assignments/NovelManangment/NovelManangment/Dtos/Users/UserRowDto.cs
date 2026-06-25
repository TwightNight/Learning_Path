using NovelManangment.Models;

namespace NovelManangment.Dtos.Users
{
    public class UserRowDto
    {
        public int Id { get; set; }
        public string UserName { get; set; } = null!;
        public string DisplayName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string? AvatarUrl { get; set; }
        public UserRole Role { get; set; }
        public bool IsDeleted { get; set; }
        public int NovelCount { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastUpdatedAt { get; set; }
    }
}
