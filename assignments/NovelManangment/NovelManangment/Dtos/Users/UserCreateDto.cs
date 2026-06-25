using NovelManangment.Models;
using System.ComponentModel.DataAnnotations;

namespace NovelManangment.Dtos.Users
{
    public class UserCreateDto
    {
        [Required, StringLength(100, MinimumLength = 3)]
        public string UserName { get; set; } = null!;

        [Required, StringLength(100)]
        public string DisplayName { get; set; } = null!;

        [Required, EmailAddress, StringLength(255)]
        public string Email { get; set; } = null!;

        [Required, StringLength(100, MinimumLength = 6)]
        public string Password { get; set; } = null!;

        [Required, Compare("Password", ErrorMessage = "Passwords do not match.")]
        public string ConfirmPassword { get; set; } = null!;

        public UserRole Role { get; set; } = UserRole.Member;

        [Url] public string? AvatarUrl { get; set; }
    }
}
