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
    public class CreateModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public CreateModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public NovelCreateDto Input { get; set; } = new();
        public List<Genre> AllGenres { get; set; } = new();
        public async Task OnGetAsync()
        {
            AllGenres = await _context.Genres.OrderBy(g => g.Name).ToListAsync();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                AllGenres = await _context.Genres.OrderBy(g => g.Name).ToListAsync();
                return Page();
            }
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdClaim))
            {
                return Unauthorized();
            }

            var publisherId = int.Parse(userIdClaim);

            var novel = new Novel
            {
                PublisherId = publisherId, // TODO: lấy từ logged-in user
                Title = Input.Title,
                AlternativeTitle = Input.AlternativeTitle,
                AuthorName = Input.AuthorName,
                ArtistName = Input.ArtistName,
                Type = Input.Type,
                Status = Input.Status,
                Description = Input.Description,
                Language = Input.Language,
                CoverUrl = Input.CoverUrl,
                Slug = GenerateSlug(Input.Title),
                CreatedAt = DateTime.Now
            };
            novel.PublishStatus = PublishStatus.Draft;

            _context.Novels.Add(novel);
            if (Input.GenreIds.Any())
            {
                var genres = await _context.Genres
                    .Where(g => Input.GenreIds.Contains(g.Id))
                    .ToListAsync();
                novel.Genres = genres;
            }
            await _context.SaveChangesAsync();

            return RedirectToPage("/Novels/MyNovels");
        }

        private string GenerateSlug(string title)
        {
            if (string.IsNullOrEmpty(title)) return "";
            var slug = title.ToLower().Trim();
            slug = System.Text.RegularExpressions.Regex.Replace(slug, @"[^a-z0-9\s-]", "");
            slug = System.Text.RegularExpressions.Regex.Replace(slug, @"\s+", "-").Trim('-');
            return $"{slug}-{Guid.NewGuid():N}"[..^26]; // giữ slug ngắn gọn
        }
    }

}