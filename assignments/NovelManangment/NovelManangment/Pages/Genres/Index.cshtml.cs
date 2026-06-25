using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using NovelManangment.Data;
using NovelManangment.Dtos.Genres;
using NovelManangment.Models;
using System.ComponentModel.DataAnnotations;

namespace NovelManangment.Pages.Genres
{
    [Authorize(Roles = "Admin")]
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        public IndexModel(ApplicationDbContext context) => _context = context;

        [BindProperty(SupportsGet = true)][FromQuery(Name = "search")] public string? Search { get; set; }
        [BindProperty(SupportsGet = true)][FromQuery(Name = "page-number")] public int PageNumber { get; set; } = 1;
        private const int PageSize = 5;

        public List<GenreRowDto> Genres { get; set; } = new();
        public int TotalCount { get; set; }
        public int TotalPages { get; set; }

        // Inline Create
        [BindProperty] public GenreUpsertDto Input { get; set; } = new();

        public async Task OnGetAsync()
        {
            await LoadAsync();
        }

        // Inline quick-create từ Index
        public async Task<IActionResult> OnPostCreateAsync()
        {
            ModelState.Remove("Page");
            ModelState.Remove("Search");
            if (!ModelState.IsValid) { await LoadAsync(); return Page(); }

            var slug = GenerateSlug(Input.Name);
            if (await _context.Genres.AnyAsync(g => g.Slug == slug || g.Name == Input.Name))
            {
                ModelState.AddModelError("Input.Name", "Genre name already exists.");
                await LoadAsync();
                return Page();
            }

            _context.Genres.Add(new Genre
            {
                Name = Input.Name,
                Slug = slug,
                Description = Input.Description
            });
            await _context.SaveChangesAsync();
            TempData["Success"] = $"Genre \"{Input.Name}\" created.";
            return RedirectToPage();
        }

        // Quick delete từ Index
        public async Task<IActionResult> OnPostDeleteAsync(int genreId)
        {
            var genre = await _context.Genres
                .Include(g => g.Novels)
                .FirstOrDefaultAsync(g => g.Id == genreId);

            if (genre is null) return NotFound();

            if (genre.Novels.Any())
            {
                TempData["Error"] = $"Cannot delete \"{genre.Name}\" — it is used by {genre.Novels.Count} novel(s).";
                return RedirectToPage();
            }

            _context.Genres.Remove(genre);
            await _context.SaveChangesAsync();
            TempData["Success"] = $"Genre \"{genre.Name}\" deleted.";
            return RedirectToPage();
        }

        private async Task LoadAsync()
        {
            var query = _context.Genres.AsQueryable();

            if (!string.IsNullOrWhiteSpace(Search))
                query = query.Where(g => g.Name.Contains(Search));

            TotalCount = await query.CountAsync();
            TotalPages = (int)Math.Ceiling(TotalCount / (double)PageSize);

            Genres = await query
                .OrderBy(g => g.Name)
                .Skip((PageNumber - 1) * PageSize)
                .Take(PageSize)
                .Select(g => new GenreRowDto
                {
                    Id = g.Id,
                    Name = g.Name,
                    Slug = g.Slug,
                    Description = g.Description,
                    NovelCount = g.Novels.Count()
                })
                .ToListAsync();
        }

        public static string GenerateSlug(string name)
        {
            var slug = name.ToLower().Trim();
            slug = System.Text.RegularExpressions.Regex.Replace(slug, @"[^a-z0-9\s-]", "");
            slug = System.Text.RegularExpressions.Regex.Replace(slug, @"\s+", "-").Trim('-');
            return slug;
        }
    }



}