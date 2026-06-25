using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using NovelManangment.Data;
using NovelManangment.Dtos.Follows;
using System.Security.Claims;

namespace NovelManangment.Pages.Follows
{
    [Authorize]
    public class IndexModel : PageModel
    {
        public ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {  
            _context = context; 
        }

        public List<FollowItemDto> Follows { get; set; } = new();
        public int TotalCount { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            if (User.Identity?.IsAuthenticated != true)
                return RedirectToPage("/login");

            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            Follows = await _context.Follows
                .Where(f => f.UserId == userId)
                .OrderByDescending(f => f.CreatedAt)
                .Select(f => new FollowItemDto 
                { 
                    Slug = f.Novel.Slug,
                    Title = f.Novel.Title,
                    CoverUrl = f.Novel.CoverUrl,
                    AuthorName = f.Novel.AuthorName,
                    Status = f.Novel.Status,
                    FollowedAt = f.CreatedAt,
                    ChapterCount = f.Novel.Volumes.SelectMany(v => v.Chapters).Count(),
                    LastUpdatedAt = f.Novel.LastUpdatedAt
                })
                .ToListAsync();

            TotalCount = Follows.Count;
            return Page();
        }

        public async Task<IActionResult> OnPostUnfollowAsync(string slug)
        {
            if (User.Identity?.IsAuthenticated != true)
                return new JsonResult(new { error = "Unauthorized" }) { StatusCode = 401 };

            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var follow = await _context.Follows
                .FirstOrDefaultAsync(f => f.Novel.Slug == slug && f.UserId == userId);

            if (follow is not null)
            {
                _context.Follows.Remove(follow);
                await _context.SaveChangesAsync();
            }

            return new JsonResult(new { success = true });
        }
    }
}
