using NovelManangment.Models;

namespace NovelManangment.Dtos.Novels
{

    public class MyNovelDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public string Slug { get; set; } = null!;
        public string? CoverUrl { get; set; }
        public NovelType Type { get; set; }
        public NovelStatus Status { get; set; }      // Ongoing/Completed/...
        public PublishStatus PublishStatus { get; set; }
        public int TotalVolumes { get; set; }
        public int TotalChapters { get; set; }
        public long TotalViews { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastUpdatedAt { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? DeletedAt { get; set; }
        public string? LastReviewNote { get; set; }
        public ReviewAction? LastReviewAction { get; set; }
    }
}
