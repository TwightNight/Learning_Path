namespace BlogApp.Application.Posts.Commands.CreatePost;

public class CreatePostRequest
{
    public required string Title { get; set; }
    public required string Content { get; set; }
}
public class CreatePostResponse
{
    public int Id { get; set; }
    public string? Title { get; set; }
    public string? Content { get; set; }
}