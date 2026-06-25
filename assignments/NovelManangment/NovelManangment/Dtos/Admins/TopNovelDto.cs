using NovelManangment.Models;

namespace NovelManangment.Dtos.Admins
{
    public class TopNovelDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public string Slug { get; set; } = null!;
        public string? CoverUrl { get; set; }
        public NovelStatus Status { get; set; }
        public NovelType Type { get; set; }
        public long TotalViews { get; set; }
        public int TotalChapters { get; set; }
    }
}
