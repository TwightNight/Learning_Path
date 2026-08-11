namespace BlogApp.Domain.Entities;

public class Post : BaseAuditableEntity
{
    public string Title { get; set; } = default!;
    public string Content { get; set; } = default!;
    public int AuthorId { get; set; }
    public DateTime? PublishedDate { get; set; }
    public bool IsPublished { get; set; } = false;
    public User Author { get; set; } = default!;
    public ICollection<Comment> Comments { get; set; } = new List<Comment>();
}