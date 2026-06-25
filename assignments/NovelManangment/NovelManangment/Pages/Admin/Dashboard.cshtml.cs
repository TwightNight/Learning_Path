using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using NovelManangment.Data;
using NovelManangment.Dtos.Admins;
using NovelManangment.Models;

namespace NovelManangment.Pages.Admin
{
    [Authorize(Roles = "Admin")]
    public class DashboardModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        public DashboardModel(ApplicationDbContext context) => _context = context;

        // ── Overview stats ──
        public int TotalNovels { get; set; }
        public int TotalChapters { get; set; }
        public int TotalUsers { get; set; }
        public int TotalPublishers { get; set; }
        public long TotalViews { get; set; }
        public int TotalGenres { get; set; }
        public int TotalVolumes { get; set; }
        public int NewUsersThisMonth { get; set; }
        public int NewNovelsThisMonth { get; set; }
        public int NewChaptersThisMonth { get; set; }

        // ── Novel by status ──
        public Dictionary<NovelStatus, int> NovelsByStatus { get; set; } = new();

        // ── Novel by type ──
        public Dictionary<NovelType, int> NovelsByType { get; set; } = new();

        // ── Top publishers ──
        public List<TopPublisherDto> TopPublishers { get; set; } = new();

        // ── Recent users ──
        public List<RecentUserDto> RecentUsers { get; set; } = new();

        // ── Most viewed novels ──
        public List<TopNovelDto> TopNovels { get; set; } = new();

        // ── Recent novels ──
        public List<RecentNovelAdminDto> RecentNovels { get; set; } = new();

        // ── Daily views last 7 days ──
        public List<DailyStatDto> WeeklyChapters { get; set; } = new();

        public async Task OnGetAsync()
        {
            var now = DateTime.Now;
            var thisMonth = new DateTime(now.Year, now.Month, 1);
            var last7days = now.AddDays(-6).Date;

            // ── Overview ──
            TotalNovels = await _context.Novels.IgnoreQueryFilters().CountAsync();
            TotalChapters = await _context.Chapters.CountAsync();
            TotalVolumes = await _context.Volumes.CountAsync();
            TotalGenres = await _context.Genres.CountAsync();
            TotalViews = await _context.Chapters.SumAsync(c => c.ViewCount);
            TotalUsers = await _context.Users.CountAsync(u => !u.IsDeleted);
            TotalPublishers = await _context.Users.CountAsync(u => !u.IsDeleted &&
                                (u.Role == UserRole.Publisher || u.Role == UserRole.Admin));

            NewUsersThisMonth = await _context.Users.CountAsync(u => u.CreatedAt >= thisMonth);
            NewNovelsThisMonth = await _context.Novels.CountAsync(n => n.CreatedAt >= thisMonth);
            NewChaptersThisMonth = await _context.Chapters.CountAsync(c => c.CreatedAt >= thisMonth);

            // ── Novel by status ──
            var statusGroups = await _context.Novels
                .GroupBy(n => n.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToListAsync();
            NovelsByStatus = statusGroups.ToDictionary(g => g.Status, g => g.Count);

            // ── Novel by type ──
            var typeGroups = await _context.Novels
                .GroupBy(n => n.Type)
                .Select(g => new { Type = g.Key, Count = g.Count() })
                .ToListAsync();
            NovelsByType = typeGroups.ToDictionary(g => g.Type, g => g.Count);

            // ── Top publishers ──
            TopPublishers = await _context.Users
                .Where(u => !u.IsDeleted &&
                    (u.Role == UserRole.Publisher || u.Role == UserRole.Admin))
                .Select(u => new TopPublisherDto
                {
                    Id = u.Id,
                    DisplayName = u.DisplayName,
                    UserName = u.UserName,
                    AvatarUrl = u.AvatarUrl,
                    TotalNovels = u.Novels.Count(),
                    TotalChapters = u.Novels.SelectMany(n => n.Chapters).Count(),
                    TotalViews = u.Novels.SelectMany(n => n.Chapters).Sum(c => c.ViewCount)
                })
                .OrderByDescending(u => u.TotalViews)
                .Take(5)
                .ToListAsync();

            // ── Recent users ──
            RecentUsers = await _context.Users
                .Where(u => !u.IsDeleted)
                .OrderByDescending(u => u.CreatedAt)
                .Take(8)
                .Select(u => new RecentUserDto
                {
                    Id = u.Id,
                    DisplayName = u.DisplayName,
                    UserName = u.UserName,
                    Email = u.Email,
                    AvatarUrl = u.AvatarUrl,
                    Role = u.Role,
                    CreatedAt = u.CreatedAt
                })
                .ToListAsync();

            // ── Top novels by views ──
            TopNovels = await _context.Novels
                .Select(n => new TopNovelDto
                {
                    Id = n.Id,
                    Title = n.Title,
                    Slug = n.Slug,
                    CoverUrl = n.CoverUrl,
                    Status = n.Status,
                    Type = n.Type,
                    TotalViews = n.Chapters.Sum(c => c.ViewCount),
                    TotalChapters = n.Chapters.Count()
                })
                .OrderByDescending(n => n.TotalViews)
                .Take(5)
                .ToListAsync();

            // ── Recent novels ──
            RecentNovels = await _context.Novels
                .Include(n => n.Publisher)
                .OrderByDescending(n => n.CreatedAt)
                .Take(8)
                .Select(n => new RecentNovelAdminDto
                {
                    Id = n.Id,
                    Title = n.Title,
                    Slug = n.Slug,
                    CoverUrl = n.CoverUrl,
                    Status = n.Status,
                    Type = n.Type,
                    PublisherName = n.Publisher.DisplayName,
                    CreatedAt = n.CreatedAt
                })
                .ToListAsync();

            // ── Chapters added per day last 7 days ──
            var chaptersPerDay = await _context.Chapters
                .Where(c => c.CreatedAt >= last7days)
                .GroupBy(c => c.CreatedAt.Date)
                .Select(g => new { Date = g.Key, Count = g.Count() })
                .ToListAsync();

            WeeklyChapters = Enumerable.Range(0, 7)
                .Select(i =>
                {
                    var date = last7days.AddDays(i);
                    return new DailyStatDto
                    {
                        Label = date.ToString("ddd dd/MM"),
                        Count = chaptersPerDay.FirstOrDefault(x => x.Date == date)?.Count ?? 0
                    };
                })
                .ToList();
        }

        public string GetTimeAgo(DateTime dt)
        {
            var s = DateTime.Now - dt;
            if (s.Days > 30) return dt.ToString("dd MMM yyyy");
            if (s.Days > 0) return $"{s.Days}d ago";
            if (s.Hours > 0) return $"{s.Hours}h ago";
            if (s.Minutes > 0) return $"{s.Minutes}m ago";
            return "Just now";
        }
    }

}