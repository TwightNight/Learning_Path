using NovelManangment.Models;

namespace NovelManangment.Dtos.Novels
{

    public class NovelCardDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public string Slug { get; set; } = null!;
        public string? CoverUrl { get; set; }
        public NovelType Type { get; set; }
        public NovelStatus Status { get; set; }
        public int TotalChapters { get; set; }
        public DateTime LastUpdated { get; set; }
        public string? LatestChapterTitle { get; set; }
    }
}
