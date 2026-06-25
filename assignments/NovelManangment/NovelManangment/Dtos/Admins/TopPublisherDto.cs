namespace NovelManangment.Dtos.Admins
{
    public class TopPublisherDto
    {
        public int Id { get; set; }
        public string DisplayName { get; set; } = null!;
        public string UserName { get; set; } = null!;
        public string? AvatarUrl { get; set; }
        public int TotalNovels { get; set; }
        public int TotalChapters { get; set; }
        public long TotalViews { get; set; }
    }
}
