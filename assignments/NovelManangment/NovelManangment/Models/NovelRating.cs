namespace NovelManangment.Models
{
    public class NovelRating
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int NovelId { get; set; }
        public int Score { get; set; } // 1-5
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public User User { get; set; }
        public Novel Novel { get; set; }
    }
}
