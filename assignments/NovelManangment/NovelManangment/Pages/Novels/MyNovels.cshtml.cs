using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using NovelManangment.Data;
using NovelManangment.Dtos.Novels;
using NovelManangment.Models;
using System.Security.Claims;

namespace NovelManangment.Pages.Novels
{
    [Authorize(Roles = "Publisher,Admin")]
    public class MyNovelsModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        public MyNovelsModel(ApplicationDbContext context) => _context = context;

        [BindProperty(SupportsGet = true)][FromQuery(Name = "search")] public string? Search { get; set; }
        [BindProperty(SupportsGet = true)][FromQuery(Name = "status")] public string? StatusFilter { get; set; }
        [BindProperty(SupportsGet = true)][FromQuery(Name = "page-number")] public int PageNumber { get; set; } = 1;

        public List<MyNovelDto> Novels { get; set; } = new();
        public int TotalCount { get; set; }
        public int TotalPages { get; set; }
        private const int PageSize = 10;

        public async Task<IActionResult> OnGetAsync()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var query = _context.Novels
                .IgnoreQueryFilters()
                .Where(n => n.PublisherId == userId)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(Search))
                query = query.Where(n => n.Title.Contains(Search));

            if (!string.IsNullOrWhiteSpace(StatusFilter) &&
                Enum.TryParse<NovelStatus>(StatusFilter, out var status))
                query = query.Where(n => n.Status == status);

            TotalCount = await query.CountAsync();
            TotalPages = (int)Math.Ceiling(TotalCount / (double)PageSize);

            Novels = await query
                .OrderByDescending(n => n.LastUpdatedAt ?? n.CreatedAt)
                .Skip((PageNumber - 1) * PageSize)
                .Take(PageSize)
                .Select(n => new MyNovelDto
                {
                    Id = n.Id,
                    Title = n.Title,
                    Slug = n.Slug,
                    CoverUrl = n.CoverUrl,
                    Type = n.Type,
                    Status = n.Status,
                    PublishStatus = n.PublishStatus,
                    TotalVolumes = n.Volumes.Count(),
                    TotalChapters = n.Chapters.Count(),
                    TotalViews = n.Chapters.Sum(c => c.ViewCount),
                    CreatedAt = n.CreatedAt,
                    LastUpdatedAt = n.LastUpdatedAt,
                    IsDeleted = n.IsDeleted,
                    DeletedAt = n.DeletedAt,
                    LastReviewNote = n.Reviews
                        .OrderByDescending(r => r.CreatedAt)
                        .Select(r => new { r.Action, r.Note, r.CreatedAt })
                        .FirstOrDefault() == null ? null
                        : n.Reviews.OrderByDescending(r => r.CreatedAt)
                            .Select(r => r.Note).FirstOrDefault(),
                    LastReviewAction = n.Reviews
                        .OrderByDescending(r => r.CreatedAt)
                        .Select(r => (ReviewAction?)r.Action)
                        .FirstOrDefault()
                                    })
                .ToListAsync();

            return Page();
        }

        public async Task<IActionResult> OnPostSubmitAsync(int novelId)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var novel = await _context.Novels
                .FirstOrDefaultAsync(n => n.Id == novelId && n.PublisherId == userId);
            if (novel is null) return NotFound();

            if (novel.PublishStatus != PublishStatus.Draft &&
                novel.PublishStatus != PublishStatus.Rejected)
            {
                TempData["Error"] = "Novel cannot be submitted in its current state.";
                return RedirectToPage();
            }

            novel.PublishStatus = PublishStatus.Pending;
            novel.LastUpdatedAt = DateTime.Now;
            await _context.SaveChangesAsync();

            TempData["Success"] = $"\"{novel.Title}\" submitted for review.";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int novelId)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var novel = await _context.Novels
                .IgnoreQueryFilters()
                .Include(n => n.Volumes).ThenInclude(v => v.Chapters)
                .FirstOrDefaultAsync(n => n.Id == novelId && n.PublisherId == userId);

            if (novel is null) return NotFound();

            foreach (var vol in novel.Volumes)
                _context.Chapters.RemoveRange(vol.Chapters);
            _context.Volumes.RemoveRange(novel.Volumes);
            _context.Novels.Remove(novel);
            await _context.SaveChangesAsync();

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostToggleDeleteAsync(int novelId)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            // IgnoreQueryFilters để tìm được cả novel đã deactivate
            var novel = await _context.Novels
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(n => n.Id == novelId && n.PublisherId == userId);

            if (novel is null) return NotFound();

            novel.IsDeleted = !novel.IsDeleted;
            novel.DeletedAt = novel.IsDeleted ? DateTime.Now : null;

            await _context.SaveChangesAsync();
            return RedirectToPage();
        }
    }

}