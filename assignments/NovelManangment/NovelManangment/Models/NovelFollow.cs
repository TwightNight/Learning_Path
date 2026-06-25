namespace NovelManangment.Models
{
    public class NovelFollow
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int NovelId { get; set; }
        public DateTime CreatedAt { get; set; }

        public User User { get; set; }
        public Novel Novel { get; set; }
    }
}
