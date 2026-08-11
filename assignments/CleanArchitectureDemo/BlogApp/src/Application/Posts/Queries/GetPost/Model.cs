namespace BlogApp.Application.Posts.Queries.GetPost;

public class GetPostRequest
{
    public int Id { get; set; }
}
public class GetPostResponse
{
    public int Id { get; set; }
    public string? Title { get; set; }
    public string? Content { get; set; }
    public DateTime? PublishedDate { get; set; }
    public bool IsPublished { get; set; }
    public AuthorDto? Author { get; set; }
    public List<CommentDto>? Comments { get; set; }
}

public class AuthorDto
{
    public int Id { get; set; }
    public string? FullName { get; set; }
}

public class CommentDto
{
    public int Id { get; set; }
    public string? Content { get; set; }
    public int UserId { get; set; }
    public string? UserFullName { get; set; }
}