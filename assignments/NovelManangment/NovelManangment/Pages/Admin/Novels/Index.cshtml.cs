using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using NovelManangment.Data;
using NovelManangment.Models;

namespace NovelManangment.Pages.Admin.Novels
{
    [Authorize(Roles = "Admin")]
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        public IndexModel(ApplicationDbContext context) => _context = context;

        [BindProperty(SupportsGet = true)][FromQuery(Name = "search")] public string? Search { get; set; }
        [BindProperty(SupportsGet = true)][FromQuery(Name = "status")] public string? StatusFilter { get; set; }
        [BindProperty(SupportsGet = true)][FromQuery(Name = "type")] public string? TypeFilter { get; set; }
        [BindProperty(SupportsGet = true)][FromQuery(Name = "show-deleted")] public string? ShowDeleted { get; set; } // "all" | "deleted" | "" (active only)
        [BindProperty(SupportsGet = true)][FromQuery(Name = "page-number")] public int PageNumber { get; set; } = 1;

        public List<AdminNovelDto> Novels { get; set; } = new();
        public int TotalCount { get; set; }
        public int TotalPages { get; set; }
        private const int PageSize = 15;

        public record AdminNovelDto(
            int Id,
            string Title,
            string Slug,
            string? CoverUrl,
            string PublisherName,
            NovelType Type,
            NovelStatus Status,
            int TotalVolumes,
            int TotalChapters,
            long TotalViews,
            int FollowCount,
            bool IsDeleted,
            DateTime? DeletedAt,
            DateTime CreatedAt,
            DateTime? LastUpdatedAt
        );

        public async Task OnGetAsync()
        {
            var query = _context.Novels
                .IgnoreQueryFilters()
                .Include(n => n.Publisher)
                .AsQueryable();

            // Search
            if (!string.IsNullOrWhiteSpace(Search))
                query = query.Where(n =>
                    n.Title.Contains(Search) ||
                    n.AuthorName.Contains(Search) ||
                    n.Publisher.DisplayName.Contains(Search));

            // Status filter
            if (!string.IsNullOrWhiteSpace(StatusFilter) &&
                Enum.TryParse<NovelStatus>(StatusFilter, out var status))
                query = query.Where(n => n.Status == status);

            // Type filter
            if (!string.IsNullOrWhiteSpace(TypeFilter) &&
                Enum.TryParse<NovelType>(TypeFilter, out var type))
                query = query.Where(n => n.Type == type);

            // Deleted filter
            query = ShowDeleted switch
            {
                "deleted" => query.Where(n => n.IsDeleted),
                "active" => query.Where(n => !n.IsDeleted),
                _ => query  // "all" — show everything
            };

            TotalCount = await query.CountAsync();
            TotalPages = (int)Math.Ceiling(TotalCount / (double)PageSize);

            Novels = await query
                .OrderByDescending(n => n.LastUpdatedAt ?? n.CreatedAt)
                .Skip((PageNumber - 1) * PageSize)
                .Take(PageSize)
                .Select(n => new AdminNovelDto(
                    n.Id,
                    n.Title,
                    n.Slug,
                    n.CoverUrl,
                    n.Publisher.DisplayName,
                    n.Type,
                    n.Status,
                    n.Volumes.Count(),
                    n.Chapters.Count(),
                    n.Chapters.Sum(c => (long)c.ViewCount),
                    n.Follows.Count(),
                    n.IsDeleted,
                    n.DeletedAt,
                    n.CreatedAt,
                    n.LastUpdatedAt
                ))
                .ToListAsync();
        }

        public async Task<IActionResult> OnPostToggleDeleteAsync(int novelId)
        {
            var novel = await _context.Novels
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(n => n.Id == novelId);

            if (novel is null) return NotFound();

            novel.IsDeleted = !novel.IsDeleted;
            novel.DeletedAt = novel.IsDeleted ? DateTime.Now : null;
            await _context.SaveChangesAsync();

            return RedirectToPage(new
            {
                Search,
                StatusFilter,
                TypeFilter,
                ShowDeleted,
                PageNumber
            });
        }

        public async Task<IActionResult> OnPostDeleteAsync(int novelId)
        {
            var novel = await _context.Novels
                .IgnoreQueryFilters()
                .Include(n => n.Volumes).ThenInclude(v => v.Chapters)
                .FirstOrDefaultAsync(n => n.Id == novelId);

            if (novel is null) return NotFound();

            foreach (var vol in novel.Volumes)
                _context.Chapters.RemoveRange(vol.Chapters);
            _context.Volumes.RemoveRange(novel.Volumes);
            _context.Novels.Remove(novel);
            await _context.SaveChangesAsync();

            return RedirectToPage(new
            {
                Search,
                StatusFilter,
                TypeFilter,
                ShowDeleted,
                PageNumber
            });
        }
    }
}