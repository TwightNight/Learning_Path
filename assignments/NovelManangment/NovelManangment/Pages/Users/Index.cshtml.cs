using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using NovelManangment.Data;
using NovelManangment.Dtos.Users;
using NovelManangment.Models;

namespace NovelManangment.Pages.Users
{
    [Authorize(Roles = "Admin")]
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        public IndexModel(ApplicationDbContext context) => _context = context;

        [BindProperty(SupportsGet = true)][FromQuery(Name = "search")] public string? Search { get; set; }
        [BindProperty(SupportsGet = true)][FromQuery(Name = "role-filter")] public string? RoleFilter { get; set; }
        [BindProperty(SupportsGet = true)][FromQuery(Name = "show-deleted")] public bool ShowDeleted { get; set; }
        [BindProperty(SupportsGet = true)][FromQuery(Name = "page-number")] public int PageNumber { get; set; } = 1;
        private const int PageSize = 15;

        public List<UserRowDto> Users { get; set; } = new();
        public int TotalCount { get; set; }
        public int TotalPages { get; set; }
        public int ActiveCount { get; set; }
        public int DeletedCount { get; set; }
        public int PublisherCount { get; set; }

        public async Task OnGetAsync() => await LoadAsync();

        public async Task<IActionResult> OnPostDeleteAsync(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user is null) return NotFound();
            user.IsDeleted = true;
            user.LastUpdatedAt = DateTime.Now;
            await _context.SaveChangesAsync();
            TempData["Success"] = $"User \"{user.DisplayName}\" deactivated.";
            return RedirectToPage(new { Search, RoleFilter, ShowDeleted, PageNumber });
        }

        public async Task<IActionResult> OnPostRestoreAsync(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user is null) return NotFound();
            user.IsDeleted = false;
            user.LastUpdatedAt = DateTime.Now;
            await _context.SaveChangesAsync();
            TempData["Success"] = $"User \"{user.DisplayName}\" restored.";
            return RedirectToPage(new { Search, RoleFilter, ShowDeleted, PageNumber });
        }

        public async Task<IActionResult> OnPostHardDeleteAsync(int userId)
        {
            var user = await _context.Users
                .Include(u => u.Novels)
                .FirstOrDefaultAsync(u => u.Id == userId);
            if (user is null) return NotFound();

            if (user.Novels.Any())
            {
                TempData["Error"] =
                    $"Cannot delete \"{user.DisplayName}\" — they own {user.Novels.Count} novel(s).";
                return RedirectToPage(new { Search, RoleFilter, ShowDeleted, PageNumber });
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            TempData["Success"] = $"User \"{user.DisplayName}\" permanently deleted.";
            return RedirectToPage(new { Search, RoleFilter, ShowDeleted, PageNumber });
        }

        private async Task LoadAsync()
        {
            ActiveCount = await _context.Users.CountAsync(u => !u.IsDeleted);
            DeletedCount = await _context.Users.CountAsync(u => u.IsDeleted);
            PublisherCount = await _context.Users.CountAsync(u =>
                !u.IsDeleted && (u.Role == UserRole.Publisher || u.Role == UserRole.Admin));

            var query = _context.Users.AsQueryable();
            if (!ShowDeleted) query = query.Where(u => !u.IsDeleted);

            if (!string.IsNullOrWhiteSpace(Search))
                query = query.Where(u =>
                    u.UserName.Contains(Search) ||
                    u.DisplayName.Contains(Search) ||
                    u.Email.Contains(Search));

            if (!string.IsNullOrWhiteSpace(RoleFilter) &&
                Enum.TryParse<UserRole>(RoleFilter, out var role))
                query = query.Where(u => u.Role == role);

            TotalCount = await query.CountAsync();
            TotalPages = (int)Math.Ceiling(TotalCount / (double)PageSize);

            Users = await query
                .OrderByDescending(u => u.CreatedAt)
                .Skip((PageNumber - 1) * PageSize)
                .Take(PageSize)
                .Select(u => new UserRowDto
                {
                    Id = u.Id,
                    UserName = u.UserName,
                    DisplayName = u.DisplayName,
                    Email = u.Email,
                    AvatarUrl = u.AvatarUrl,
                    Role = u.Role,
                    IsDeleted = u.IsDeleted,
                    NovelCount = u.Novels.Count(),
                    CreatedAt = u.CreatedAt,
                    LastUpdatedAt = u.LastUpdatedAt
                })
                .ToListAsync();
        }
    }


}