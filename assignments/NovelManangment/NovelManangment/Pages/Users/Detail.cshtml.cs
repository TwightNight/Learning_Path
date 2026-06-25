using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using NovelManangment.Data;
using NovelManangment.Models;
using System.Security.Claims;

namespace NovelManangment.Pages.Users
{
    [Authorize]
    public class DetailModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        public DetailModel(ApplicationDbContext context) => _context = context;

        public User User { get; set; } = null!;
        public List<Novel> Novels { get; set; } = new();
        public long TotalViews { get; set; }
        public int TotalChapters { get; set; }

        public async Task<IActionResult> OnGetAsync(int userId)
        {
            var currentUserId = int.Parse(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var isAdmin = HttpContext.User.IsInRole("Admin");

            // Chỉ admin hoặc chính chủ mới xem được
            if (!isAdmin && currentUserId != userId)
                return Forbid();

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == userId);
            if (user is null) return NotFound();
            User = user;

            Novels = await _context.Novels
                .Where(n => n.PublisherId == userId)
                .OrderByDescending(n => n.LastUpdatedAt ?? n.CreatedAt)
                .ToListAsync();

            TotalViews = await _context.Chapters
                .Where(c => c.Novel.PublisherId == userId)
                .SumAsync(c => c.ViewCount);

            TotalChapters = await _context.Chapters
                .CountAsync(c => c.Novel.PublisherId == userId);

            return Page();
        }

        public string GetTimeAgo(DateTime dt)
        {
            var s = DateTime.Now - dt;
            if (s.Days > 365) return $"{s.Days / 365}y ago";
            if (s.Days > 30) return $"{s.Days / 30}mo ago";
            if (s.Days > 0) return $"{s.Days}d ago";
            if (s.Hours > 0) return $"{s.Hours}h ago";
            if (s.Minutes > 0) return $"{s.Minutes}m ago";
            return "Just now";
        }
    }
}