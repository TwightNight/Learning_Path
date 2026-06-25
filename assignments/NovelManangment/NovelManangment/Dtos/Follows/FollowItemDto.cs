using NovelManangment.Models;

namespace NovelManangment.Dtos.Follows
{
    public class FollowItemDto
    {
        public string Slug {  get; set; }
        public string Title { get; set; }
        public string? CoverUrl { get; set; }
        public string AuthorName {  get; set; }
        public NovelStatus Status { get; set; }
        public DateTime FollowedAt { get; set; }
        public int ChapterCount { get; set; }
        public DateTime? LastUpdatedAt { get; set; }
    }
}
