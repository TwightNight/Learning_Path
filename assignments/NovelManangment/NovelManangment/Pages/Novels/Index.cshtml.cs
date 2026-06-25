using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using NovelManangment.Data;
using NovelManangment.Dtos.Novels;
using NovelManangment.Models;
using System.Text.RegularExpressions;

namespace NovelManangment.Pages.Novels
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        // --- Filter params ---
        [BindProperty(SupportsGet = true)][FromQuery(Name = "search")] public string? Search { get; set; }
        [BindProperty(SupportsGet = true)][FromQuery(Name = "letter")] public string? Letter { get; set; }
        [BindProperty(SupportsGet = true)][FromQuery(Name = "types")] public List<NovelType> Types { get; set; } = new();
        [BindProperty(SupportsGet = true)][FromQuery(Name = "statuses")] public List<NovelStatus> Statuses { get; set; } = new();
        //[BindProperty(SupportsGet = true)] public List<int> GenreIds { get; set; } = new();
        [BindProperty(SupportsGet = true)][FromQuery(Name = "genres")] public List<string> GenreSlugs { get; set; } = new();
        [BindProperty(SupportsGet = true)][FromQuery(Name = "sort")] public string Sort { get; set; } = "az";
        [BindProperty(SupportsGet = true)][FromQuery(Name = "page-number")] public int PageNumber { get; set; } = 1;

        // --- Output ---
        public List<NovelCardDto> Novels { get; set; } = new();
        public List<Genre> AllGenres { get; set; } = new();
        public int TotalPages { get; set; }
        public int TotalCount { get; set; }
        private const int PageSize = 24;

        public async Task OnGetAsync()
        {
            AllGenres = await _context.Genres.OrderBy(g => g.Name).ToListAsync();

            var query = _context.Novels
                .Include(n => n.Genres)
                .Where(n => !n.Publisher.IsDeleted && n.PublishStatus == PublishStatus.Approved)
                .AsQueryable();

            // Search
            if (!string.IsNullOrWhiteSpace(Search))
                query = query.Where(n => n.Title.Contains(Search) || (n.AlternativeTitle != null && n.AlternativeTitle.Contains(Search)));

            // Letter filter
            if (!string.IsNullOrWhiteSpace(Letter) && Letter != "ALL")
            {
                    query = query.Where(n => n.Title.StartsWith(Letter));
            }

            // Type filter
            if (Types.Any())
                query = query.Where(n => Types.Contains(n.Type));

            // Status filter
            if (Statuses.Any())
                query = query.Where(n => Statuses.Contains(n.Status));

            // Genre filter
            //if (GenreIds.Any())
            //    query = query.Where(n => n.Genres.Any(g => GenreIds.Contains(g.Id)));
            // Genre filter slug
            if (GenreSlugs.Any())
                query = query.Where(n => n.Genres.Any(g => GenreSlugs.Contains(g.Slug)));


            // Sort
            query = Sort switch
            {
                "az" => query.OrderBy(n => n.Title),
                "za" => query.OrderByDescending(n => n.Title),
                "newest" => query.OrderByDescending(n => n.CreatedAt),
                "updated" => query.OrderByDescending(n => n.LastUpdatedAt ?? n.CreatedAt),
                _ => query.OrderBy(n => n.Title)
            };

            TotalCount = await query.CountAsync();
            TotalPages = (int)Math.Ceiling(TotalCount / (double)PageSize);

            Novels = await query
                .Skip((PageNumber - 1) * PageSize)
                .Take(PageSize)
                .Select(n => new NovelCardDto
                {
                    Id = n.Id,
                    Title = n.Title,
                    Slug = n.Slug,
                    CoverUrl = n.CoverUrl,
                    Type = n.Type,
                    Status = n.Status,
                    TotalChapters = n.Chapters.Count(),
                    LastUpdated = n.LastUpdatedAt ?? n.CreatedAt,
                    LatestChapterTitle = n.Chapters
                        .OrderByDescending(c => c.ChapterNumber)
                        .Select(c => c.Title)
                        .FirstOrDefault()
                })
                .ToListAsync();
        }
    }

}