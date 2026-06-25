using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using NovelManangment.Data;
using NovelManangment.Models;

namespace NovelManangment.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }


        public List<RecentNovelDto> RecentNovels { get; set; } = new List<RecentNovelDto>();
        public List<SiteAlert> ActiveAlerts { get; set; } = new();

        public async Task OnGetAsync()
        {


            RecentNovels = await _context.Novels
                .Include(n => n.Publisher)
                .Include(n => n.Genres)
                .Where(n => !n.Publisher.IsDeleted && n.PublishStatus == PublishStatus.Approved)
                .OrderByDescending(n => n.LastUpdatedAt ?? n.CreatedAt)
                .Take(10)
                .Select(n => new RecentNovelDto
                {
                    Title = n.Title,
                    AuthorName = n.AuthorName,
                    CoverUrl = n.CoverUrl,
                    Slug = n.Slug,
                    Status = n.Status,
                    Type = n.Type,
                    TotalChapters = n.Chapters.Count(),
                    LastUpdated = n.LastUpdatedAt ?? n.CreatedAt
                })
                .ToListAsync();

            ActiveAlerts = await _context.SiteAlerts
                .Where(a => a.IsActive && (a.ExpiresAt == null || a.ExpiresAt > DateTime.Now))
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();
        }

        public string GetTimeAgo(DateTime datetime)
        {
            TimeSpan span = DateTime.Now - datetime;
            if (span.Days > 365) return $"{span.Days / 365} year(s) ago";
            if (span.Days > 30) return $"{span.Days / 30} month(s) ago";
            if (span.Days > 0) return $"{span.Days} day(s) ago";
            if (span.Hours > 0) return $"{span.Hours} hour(s) ago";
            if (span.Minutes > 0) return $"{span.Minutes} minute(s) ago";
            return "Just now";
        }
    }

    public class RecentNovelDto
    {
        public string Title { get; set; } = null!;
        public string AuthorName { get; set; } = null!;
        public string Slug { get; set; } = null!;
        public string? CoverUrl { get; set; }
        public NovelStatus Status { get; set; }
        public NovelType Type { get; set; }
        public int TotalChapters { get; set; }
        public DateTime LastUpdated { get; set; }
    }
}