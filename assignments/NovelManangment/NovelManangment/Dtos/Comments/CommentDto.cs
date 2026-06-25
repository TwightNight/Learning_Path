namespace NovelManangment.Dtos.Comments
{
    public class CommentDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string? AvatarUrl {  get; set; }
        public string DisplayName { get; set; }
        public string Content { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? ChapterTitle { get; set; }
        public string? ChapterSlug { get; set; }

    }
}
