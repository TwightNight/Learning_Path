using NovelManangment.Models;

namespace NovelManangment.Dtos.Admins
{
    public class RecentUserDto
    {
        public int Id { get; set; }
        public string DisplayName { get; set; } = null!;
        public string UserName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string? AvatarUrl { get; set; }
        public UserRole Role { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
