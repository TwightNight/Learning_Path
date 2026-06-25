namespace NovelManangment.Dtos.Novels
{
    public class ChapterRowDto
    {
        public string Slug { get; set; } = null!;
        public string Title { get; set; } = null!;
        public decimal ChapterNumber { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
