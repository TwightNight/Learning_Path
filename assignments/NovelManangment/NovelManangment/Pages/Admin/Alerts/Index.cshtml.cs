using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using NovelManangment.Data;
using NovelManangment.Models;

namespace NovelManangment.Pages.Admin.Alerts
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        public IndexModel(ApplicationDbContext context) => _context = context;

        public List<SiteAlert> Alerts { get; set; } = new();

        public async Task OnGetAsync()
        {
            Alerts = await _context.SiteAlerts
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();
        }

        // Tạo mới
        public async Task<IActionResult> OnPostCreateAsync(
            string title, string content, AlertType type, bool isActive, DateTime? expiresAt)
        {
            if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(content))
                return BadRequest();

            _context.SiteAlerts.Add(new SiteAlert
            {
                Title = title.Trim(),
                Content = content.Trim(),
                Type = type,
                IsActive = isActive,
                ExpiresAt = expiresAt,
                CreatedAt = DateTime.Now
            });
            await _context.SaveChangesAsync();
            return RedirectToPage();
        }

        // Toggle active
        public async Task<IActionResult> OnPostToggleAsync(int id)
        {
            var alert = await _context.SiteAlerts.FindAsync(id);
            if (alert is null) return NotFound();
            alert.IsActive = !alert.IsActive;
            await _context.SaveChangesAsync();
            return RedirectToPage();
        }

        // Xóa
        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            var alert = await _context.SiteAlerts.FindAsync(id);
            if (alert is not null)
            {
                _context.SiteAlerts.Remove(alert);
                await _context.SaveChangesAsync();
            }
            return RedirectToPage();
        }
    }
}