namespace BlogApp.Application.Posts.Commands.UpdatePost;

public class UpdatePostRequest
{
    // FastEndpoints tự bind từ route "{Id}" vào property cùng tên (case-insensitive)
    public int Id { get; set; }
    public required string Title { get; set; }
    public required string Content { get; set; }
}

public class UpdatePostResponse
{
    public int Id { get; set; }
    public string? Title { get; set; }
    public string? Content { get; set; }
    public DateTime LastModified { get; set; }
}