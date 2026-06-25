namespace NovelManangment.Models
{
    public enum AlertType { Info, Warning, Danger, Success }

    public class SiteAlert
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public string Content { get; set; } = null!;
        public AlertType Type { get; set; } = AlertType.Info;
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; }
        public DateTime? ExpiresAt { get; set; }
    }
}