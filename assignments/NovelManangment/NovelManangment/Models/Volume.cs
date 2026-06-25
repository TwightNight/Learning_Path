namespace NovelManangment.Models
{
    public class Volume
    {
        public int Id { get; set; }
        public int NovelId { get; set; }
        public string Title { get; set; } = null!;
        public int VolumeNumber { get; set; }
        public string? Description { get; set; }
        public VolumeType Type { get; set; }
        public string? PdfUrl { get; set; }
        public string? CoverUrl { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastUpdatedAt { get; set; }

        public Novel Novel { get; set; }
        public ICollection<Chapter> Chapters { get; set; }

    }

    public enum VolumeType
    {
        Normal = 0,
        Extra = 1,
        Illustration = 2,
        Announcement = 3,
    }
}
