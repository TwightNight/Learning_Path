using NovelManangment.Models;

namespace NovelManangment.Dtos.Novels
{
    public class PendingNovelDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public string Slug { get; set; } = null!;
        public string? CoverUrl { get; set; }
        public NovelType Type { get; set; }
        public NovelStatus NovelStatus { get; set; }      // Ongoing/Completed/...
        public PublishStatus PublishStatus { get; set; }
        //public NovelStatus Status { get; set; }
        public string PublisherName { get; set; } = null!;
        public int PublisherId { get; set; }
        public List<string> Genres { get; set; } = new();
        public int TotalChapters { get; set; }
        public DateTime CreatedAt { get; set; }
        public ReviewAction? ReviewAction { get; set; }
        public string? LastReviewNote { get; set; }
    }
}
