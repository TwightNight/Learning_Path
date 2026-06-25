using NovelManangment.Models;
using NovelManangment.Pages.Novels;

namespace NovelManangment.Dtos.Novels
{

    public class VolumeTreeDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public int VolumeNumber { get; set; }
        public VolumeType Type { get; set; }
        public string? PdfUrl { get; set; }
        public string? Description { get; set; }
        public string? CoverUrl { get; set; }
        public List<ChapterTreeDto> Chapters { get; set; } = new();
    }
}
