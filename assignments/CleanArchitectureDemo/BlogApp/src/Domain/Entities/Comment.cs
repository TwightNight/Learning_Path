namespace BlogApp.Domain.Entities;

public class Comment : BaseAuditableEntity
{
    public string Content { get; set; } = default!;
    public int PostId { get; set; }
    public int UserId { get; set; }
    public Post Post { get; set; } = default!;
    public User User { get; set; } = default!;
}