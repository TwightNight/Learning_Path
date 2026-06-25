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
    public class EditModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        public EditModel(ApplicationDbContext context) => _context = context;

        public Genre Genre { get; set; } = null!;
        public List<Novel> RelatedNovels { get; set; } = new();

        [BindProperty] public GenreUpsertDto Input { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(int genreId)
        {
            var genre = await _context.Genres
                .Include(g => g.Novels)
                .FirstOrDefaultAsync(g => g.Id == genreId);
            if (genre is null) return NotFound();

            Genre = genre;
            RelatedNovels = genre.Novels.Take(10).ToList();
            Input = new GenreUpsertDto
            {
                Name = genre.Name,
                Description = genre.Description
            };
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int genreId)
        {
            if (!ModelState.IsValid)
            {
                Genre = (await _context.Genres.FindAsync(genreId))!;
                return Page();
            }

            var genre = await _context.Genres.FindAsync(genreId);
            if (genre is null) return NotFound();

            // Check duplicate name (exclude self)
            var duplicate = await _context.Genres
                .AnyAsync(g => g.Name == Input.Name && g.Id != genreId);
            if (duplicate)
            {
                ModelState.AddModelError("Input.Name", "Genre name already exists.");
                Genre = genre;
                return Page();
            }

            genre.Name = Input.Name;
            genre.Slug = IndexModel.GenerateSlug(Input.Name);
            genre.Description = Input.Description;
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Genre \"{genre.Name}\" updated.";
            return RedirectToPage("/Genres/Index");
        }
    }
}