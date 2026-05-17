using System.ComponentModel.DataAnnotations;

namespace ContosoUniversity.Models
{
    public class User
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Username { get; set; }

        [Required]
        public string Password { get; set; }

        [StringLength(50)]
        public string Role { get; set; } = "Admin";
    }
}