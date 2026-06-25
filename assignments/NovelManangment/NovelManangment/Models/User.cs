namespace NovelManangment.Models
{
    public class User
    {
        public int Id { get; set; }
        public string UserName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
        public string DisplayName { get; set; } = null!;
        public string? AvatarUrl { get; set; }
        public UserRole Role { get; set; }
        public DateTime CreatedAt { get; set; } 
        public DateTime? LastUpdatedAt { get; set; }
        public bool IsDeleted { get; set; }
        public ICollection<Novel> Novels { get; set; }
        public ICollection<NovelFollow> Follows { get; set; }
        public ICollection<NovelRating> Ratings { get; set; }
        public ICollection<Comment> Comments { get; set; }
        public ICollection<NovelReview> Reviews { get; set; }

    }

    public enum UserRole
    {
        Member = 0,
        Publisher = 1,
        Moderator = 2,
        Admin = 3
    }
}
