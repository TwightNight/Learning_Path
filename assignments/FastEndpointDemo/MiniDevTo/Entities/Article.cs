using MiniDevTo.Common;

namespace MiniDevTo.Entities;

public class Article : BaseEntity
{
    public int AuthorId { get; set; }
    public string AuthorName { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedOn { get; set; }
    public bool IsApproved { get; set; } = true;
    public string? RejectionReason { get; set; }

    public Author? Author { get; set; }
}