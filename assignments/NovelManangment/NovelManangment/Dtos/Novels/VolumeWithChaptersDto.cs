using NovelManangment.Models;

namespace NovelManangment.Dtos.Novels
{
    public class VolumeWithChaptersDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public string? CoverUrl { get; set; }
        public string? PdfUrl { get; set; }
        public VolumeType Type { get; set; }
        public List<ChapterRowDto> Chapters { get; set; } = new();
        private const int Preview = 5;
        public List<ChapterRowDto> PreviewChapters => Chapters.Take(Preview).ToList();
        public int HiddenCount => Math.Max(0, Chapters.Count - Preview);
    }
}
