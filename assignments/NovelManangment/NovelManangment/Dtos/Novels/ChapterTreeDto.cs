namespace NovelManangment.Dtos.Novels
{

    public class ChapterTreeDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public string Slug { get; set; } = null!;
        public decimal ChapterNumber { get; set; }
    }
}
