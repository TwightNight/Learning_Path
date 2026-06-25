using NovelManangment.Models;
using System.ComponentModel.DataAnnotations;

namespace NovelManangment.Dtos.Users
{

    public class UserEditDto
    {
        public int UserId { get; set; }

        [Required, StringLength(100)]
        public string DisplayName { get; set; } = null!;

        [Required, EmailAddress, StringLength(255)]
        public string Email { get; set; } = null!;

        public UserRole Role { get; set; }

        [Url] public string? AvatarUrl { get; set; }

        [StringLength(100, MinimumLength = 6)]
        public string? NewPassword { get; set; }

        [Compare("NewPassword", ErrorMessage = "Passwords do not match.")]
        public string? ConfirmNewPassword { get; set; }
    }
}
