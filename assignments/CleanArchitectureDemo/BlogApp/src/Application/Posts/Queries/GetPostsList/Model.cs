namespace BlogApp.Application.Posts.Queries.GetPostsList;

public class GetPostsRequest
{
    public int? AuthorId { get; set; } // Query param: /posts?authorId=5
}

public class GetPostsResponse
{
    public int Id { get; set; }
    public string? Title { get; set; }
    public string? Content { get; set; }
    public DateTime? PublishedDate { get; set; }
    public bool IsPublished { get; set; }
    public int AuthorId { get; set; }
    public string? AuthorFullName { get; set; }
    public int CommentsCount { get; set; }
}