namespace NovelManangment.Models
{
    public class Novel
    {
        public int Id { get; set; }
        public int PublisherId { get; set; }
        public string Title { get; set; } = null!;
        public string? AlternativeTitle { get; set; }
        public string Slug { get; set; } = null!;
        public string Description { get; set; } = null!;
        public string Language { get; set; } = null!;
        public string? CoverUrl { get; set; }
        public NovelType Type { get; set; }
        public NovelStatus Status { get; set; }
        public string AuthorName { get; set; } = null!;
        public string? ArtistName { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastUpdatedAt { get; set; }
        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }
        public PublishStatus PublishStatus { get; set; } = PublishStatus.Draft;
        public User Publisher { get; set; }
        public ICollection<Genre> Genres { get; set; }
        public ICollection<Volume> Volumes { get; set; }
        public ICollection<Chapter> Chapters { get; set; }
        public ICollection<NovelFollow> Follows { get; set; }
        public ICollection<NovelRating> Ratings { get; set; }
        public ICollection<Comment> Comments { get; set; }
        public ICollection<NovelReview> Reviews { get; set; }
    }

    public enum NovelType
    {
        Original = 0,
        Raw = 1,
        MachineTranslated = 2,
        HumanTranslated = 3,
        Parody = 4,
    }

    public enum NovelStatus
    {
        Ongoing = 0,
        Completed = 1,
        Hiatus = 2,
        Dropped = 3
    }

    public enum PublishStatus
    {
        Draft = 0, // Publisher chưa submit
        Pending = 1, // Đã submit, chờ duyệt
        Approved = 2, // Đã duyệt, hiện cho reader
        Rejected = 3  // Bị từ chối / cần sửa
    }
}
