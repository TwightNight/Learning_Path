namespace NovelManangment.Models
{
    public class Genre
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string Slug { get; set; } = null!;
        public string? Description { get; set; }
        public ICollection<Novel> Novels { get; set; }
    }
}
