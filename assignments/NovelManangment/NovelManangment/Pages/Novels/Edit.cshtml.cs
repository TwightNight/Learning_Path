using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using NovelManangment.Data;
using NovelManangment.Dtos.Novels;
using NovelManangment.Models;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace NovelManangment.Pages.Novels
{
    [Authorize(Roles = "Publisher,Admin")]
    public class EditModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        public EditModel(ApplicationDbContext context) => _context = context;

        [BindProperty]
        public NovelEditDto Input { get; set; } = new();
        public List<Genre> AllGenres { get; set; } = new();
        public Novel Novel { get; set; } = null!;

        public async Task<IActionResult> OnGetAsync(int novelId)
        {
            var novel = await _context.Novels
                .Include(n => n.Genres)
                .FirstOrDefaultAsync(n => n.Id == novelId);

            if (novel is null) return NotFound();
            if (!CanEdit(novel)) return Forbid();

            Novel = novel;
            AllGenres = await _context.Genres.OrderBy(g => g.Name).ToListAsync();

            // Map sang DTO
            Input = new NovelEditDto
            {
                NovelId = novel.Id,
                Title = novel.Title,
                AlternativeTitle = novel.AlternativeTitle,
                AuthorName = novel.AuthorName,
                ArtistName = novel.ArtistName,
                Type = novel.Type,
                Status = novel.Status,
                Description = novel.Description,
                Language = novel.Language,
                CoverUrl = novel.CoverUrl,
                GenreIds = novel.Genres.Select(g => g.Id).ToList()
            };

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                AllGenres = await _context.Genres.OrderBy(g => g.Name).ToListAsync();
                Novel = (await _context.Novels.FindAsync(Input.NovelId))!;
                return Page();
            }

            var novel = await _context.Novels
                .Include(n => n.Genres)
                .FirstOrDefaultAsync(n => n.Id == Input.NovelId);

            if (novel is null) return NotFound();
            if (!CanEdit(novel)) return Forbid();

            // Cập nhật fields
            novel.Title = Input.Title;
            novel.AlternativeTitle = Input.AlternativeTitle;
            novel.AuthorName = Input.AuthorName;
            novel.ArtistName = Input.ArtistName;
            novel.Type = Input.Type;
            novel.Status = Input.Status;
            novel.Description = Input.Description;
            novel.Language = Input.Language;
            novel.CoverUrl = Input.CoverUrl;
            novel.LastUpdatedAt = DateTime.Now;

            // Cập nhật Genres
            novel.Genres.Clear();
            if (Input.GenreIds.Any())
            {
                var genres = await _context.Genres
                    .Where(g => Input.GenreIds.Contains(g.Id))
                    .ToListAsync();
                foreach (var g in genres)
                    novel.Genres.Add(g);
            }
            novel.PublishStatus = PublishStatus.Pending;
            await _context.SaveChangesAsync();
            TempData["Success"] = $"Novel \"{novel.Title}\" updated successfully.";
            return RedirectToPage("/Novels/MyNovels");
        }

        private bool CanEdit(Novel novel)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            return novel.PublisherId == userId || User.IsInRole("Admin");
        }
    }


}