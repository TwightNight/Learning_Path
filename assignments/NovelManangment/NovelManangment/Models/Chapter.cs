namespace NovelManangment.Models
{
    public class Chapter
    {
        public int Id { get; set; }
        public int NovelId { get; set; }
        public int VolumeId { get; set; }
        public string Title { get; set; } = null!;
        public string Slug { get; set; } = null!;
        public decimal ChapterNumber { get; set; }
        public string? Content { get; set; }
        public int WordCount { get; set; }
        public long ViewCount { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastUpdatedAt { get; set; }
        public Volume Volume { get; set; }
        public Novel Novel { get; set; }
        public ICollection<Comment> Comments { get; set; }

    }
}
