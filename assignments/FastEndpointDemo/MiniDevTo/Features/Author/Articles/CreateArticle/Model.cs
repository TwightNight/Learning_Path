namespace MiniDevTo.Features.Author.Articles.CreateArticle;

//request
public class Request
{
    public string Title { get; set; }
    public string Content { get; set; }
    public int AuthorId { get; set; }
}

public class Response
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string AuthorName { get; set; } = string.Empty;
    public DateTime CreatedOn { get; set; }
}