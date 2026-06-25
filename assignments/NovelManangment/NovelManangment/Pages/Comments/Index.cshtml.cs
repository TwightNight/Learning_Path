using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using NovelManangment.Data;
using System.Security.Claims;

namespace NovelManangment.Pages.Comments
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        public IndexModel(ApplicationDbContext context) => _context = context;

        // Dummy GET — trang này chỉ dùng làm API endpoint
        public IActionResult OnGet() => NotFound();

        // ── Edit ──────────────────────────────────────────────
        public async Task<IActionResult> OnPostEditAsync(int id, string content)
        {
            if (!User.Identity?.IsAuthenticated ?? true)
                return new JsonResult(new { error = "Unauthorized" }) { StatusCode = 401 };

            if (string.IsNullOrWhiteSpace(content) || content.Length > 2000)
                return new JsonResult(new { error = "Content invalid" }) { StatusCode = 400 };

            var comment = await _context.Comments.FindAsync(id);
            if (comment is null)
                return new JsonResult(new { error = "Not found" }) { StatusCode = 404 };

            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            if (comment.UserId != userId)
                return new JsonResult(new { error = "Forbidden" }) { StatusCode = 403 };

            comment.Content = content.Trim();
            // Nếu model có UpdatedAt thì set: comment.UpdatedAt = DateTime.Now;
            await _context.SaveChangesAsync();

            return new JsonResult(new { success = true, content = comment.Content });
        }

        // ── Delete ────────────────────────────────────────────
        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            if (!User.Identity?.IsAuthenticated ?? true)
                return new JsonResult(new { error = "Unauthorized" }) { StatusCode = 401 };

            var comment = await _context.Comments.FindAsync(id);
            if (comment is null)
                return new JsonResult(new { error = "Not found" }) { StatusCode = 404 };

            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            if (comment.UserId != userId)
                return new JsonResult(new { error = "Forbidden" }) { StatusCode = 403 };

            _context.Comments.Remove(comment);
            await _context.SaveChangesAsync();

            return new JsonResult(new { success = true });
        }
    }
}