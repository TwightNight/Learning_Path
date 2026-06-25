namespace NovelManangment.Dtos.Genres
{
    public class GenreRowDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string Slug { get; set; } = null!;
        public string? Description { get; set; }
        public int NovelCount { get; set; }
    }
}
