namespace NovelManangment.Models
{
    public class NovelReview
    {
        public int Id { get; set; }
        public int NovelId { get; set; }
        public int ModeratorId { get; set; }
        public ReviewAction Action { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? Note { get; set; }

        public Novel Novel { get; set; }
        public User Moderator { get; set; }
    }

    public enum ReviewAction
    {
        Approved = 0,
        Rejected = 1,
        RequestRevision = 2
    }
}
