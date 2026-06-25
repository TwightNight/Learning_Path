namespace NovelManangment.Models
{
    public class Comment
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int? NovelId { get; set; }
        public int? ChapterId { get; set; }
        public string Content { get; set; } = null!;
        public DateTime CreatedAt { get; set; }

        public User User { get; set; }
        public Novel? Novel { get; set; }
        public Chapter? Chapter { get; set; }
    }
}
