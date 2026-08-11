using System.ComponentModel.DataAnnotations.Schema;
using MiniDevTo.Common;

namespace MiniDevTo.Entities;

public class Author : BaseEntity
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    [NotMapped]
    public string FullName => $"{FirstName} {LastName}";
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public DateOnly SignUpDate { get; set; }
    public List<Article> Articles { get; set; } = new List<Article>();
}