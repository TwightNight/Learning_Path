using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using NovelManangment.Data;
using NovelManangment.Dtos.Novels;
using NovelManangment.Models;
using System.Security.Claims;

namespace NovelManangment.Pages.Moderator
{
    [Authorize(Roles = "Moderator,Admin")]
    public class DashboardModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        public DashboardModel(ApplicationDbContext context) => _context = context;

        public List<PendingNovelDto> PendingNovels { get; set; } = new();
        public List<PendingNovelDto> RecentlyReviewed { get; set; } = new();
        public int PendingCount { get; set; }
        public int ApprovedToday { get; set; }
        public int RejectedToday { get; set; }
        public int TotalReviewed { get; set; }

        public async Task OnGetAsync()
        {
            var modId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var today = DateTime.Today;

            PendingCount = await _context.Novels
                .CountAsync(n => n.PublishStatus == PublishStatus.Pending);

            ApprovedToday = await _context.NovelReviews
                .CountAsync(r => r.ModeratorId == modId
                              && r.Action == ReviewAction.Approved
                              && r.CreatedAt >= today);

            RejectedToday = await _context.NovelReviews
                .CountAsync(r => r.ModeratorId == modId
                              && r.Action == ReviewAction.Rejected
                              && r.CreatedAt >= today);

            PendingNovels = await _context.Novels
                .Where(n => n.PublishStatus == PublishStatus.Pending)
                .Include(n => n.Publisher)
                .Include(n => n.Genres)
                .OrderBy(n => n.CreatedAt)
                .Select(n => new PendingNovelDto
                {
                    Id = n.Id,
                    Title = n.Title,
                    Slug = n.Slug,
                    CoverUrl = n.CoverUrl,
                    Type = n.Type,
                    NovelStatus = n.Status,
                    PublishStatus = n.PublishStatus,
                    PublisherName = n.Publisher.DisplayName,
                    PublisherId = n.PublisherId,
                    Genres = n.Genres.Select(g => g.Name).ToList(),
                    TotalChapters = n.Chapters.Count(),
                    CreatedAt = n.CreatedAt,
                    LastReviewNote = n.Reviews
                        .OrderByDescending(r => r.CreatedAt)
                        .Select(r => r.Note)
                        .FirstOrDefault()
                })
                .ToListAsync();

            RecentlyReviewed = await _context.NovelReviews
                .Where(r => r.ModeratorId == modId)
                .OrderByDescending(r => r.CreatedAt)
                .Take(10)
                .Select(r => new PendingNovelDto
                {
                    Id = r.Novel.Id,
                    Title = r.Novel.Title,
                    Slug = r.Novel.Slug,
                    CoverUrl = r.Novel.CoverUrl,
                    Type = r.Novel.Type,
                    NovelStatus = r.Novel.Status,
                    PublisherName = r.Novel.Publisher.DisplayName,
                    PublisherId = r.Novel.PublisherId,
                    Genres = r.Novel.Genres.Select(g => g.Name).ToList(),
                    TotalChapters = r.Novel.Chapters.Count(),
                    CreatedAt = r.CreatedAt,
                    ReviewAction = r.Action,
                    LastReviewNote = r.Note
                })
                .ToListAsync();
        }

        // ── Approve ──
        public async Task<IActionResult> OnPostApproveAsync(int novelId)
        {
            var modId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var novel = await _context.Novels.FindAsync(novelId);
            if (novel is null) return NotFound();

            novel.PublishStatus = PublishStatus.Approved; // ← chỉ sửa PublishStatus
            novel.LastUpdatedAt = DateTime.Now;
            // NovelStatus (Ongoing/Completed...) giữ nguyên do publisher set

            _context.NovelReviews.Add(new NovelReview
            {
                NovelId = novelId,
                ModeratorId = modId,
                Action = ReviewAction.Approved,
                Note = "Novel approved. Now visible to readers.",
                CreatedAt = DateTime.Now
            });

            await _context.SaveChangesAsync();
            TempData["Success"] = "Novel approved and is now live.";
            return RedirectToPage();
        }

        // ── Reject ──
        public async Task<IActionResult> OnPostRejectAsync(int novelId, string note)
        {
            var modId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var novel = await _context.Novels.FindAsync(novelId);
            if (novel is null) return NotFound();

            novel.PublishStatus = PublishStatus.Rejected; // ← chỉ sửa PublishStatus
            novel.LastUpdatedAt = DateTime.Now;

            _context.NovelReviews.Add(new NovelReview
            {
                NovelId = novelId,
                ModeratorId = modId,
                Action = ReviewAction.Rejected,
                Note = note?.Trim(),
                CreatedAt = DateTime.Now
            });

            await _context.SaveChangesAsync();
            TempData["Success"] = "Novel rejected. Publisher has been notified.";
            return RedirectToPage();
        }

        // ── Request Revision ──
        public async Task<IActionResult> OnPostRequestRevisionAsync(int novelId, string note)
        {
            if (string.IsNullOrWhiteSpace(note))
            {
                TempData["Error"] = "Please provide revision notes.";
                return RedirectToPage();
            }

            var modId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var novel = await _context.Novels.FindAsync(novelId);
            if (novel is null) return NotFound();

            novel.PublishStatus = PublishStatus.Rejected; // ← cần sửa lại
            novel.LastUpdatedAt = DateTime.Now;

            _context.NovelReviews.Add(new NovelReview
            {
                NovelId = novelId,
                ModeratorId = modId,
                Action = ReviewAction.RequestRevision,
                Note = note.Trim(),
                CreatedAt = DateTime.Now
            });

            await _context.SaveChangesAsync();
            TempData["Success"] = "Revision requested. Publisher has been notified.";
            return RedirectToPage();
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