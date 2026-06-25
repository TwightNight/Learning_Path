using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using NovelManangment.Data;
using NovelManangment.Dtos.Novels;
using NovelManangment.Models;
using System.ComponentModel.DataAnnotations;

namespace NovelManangment.Pages.Novels
{
    [Authorize(Roles = "Publisher,Admin")]
    public class ManageModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        public ManageModel(ApplicationDbContext context) => _context = context;

        public Novel Novel { get; set; } = null!;
        public List<VolumeTreeDto> Tree { get; set; } = new();

        // ── Load ──────────────────────────────────────────────────
        public async Task<IActionResult> OnGetAsync(int novelId)
        {
            var novel = await _context.Novels
                .Include(n => n.Publisher)
                .FirstOrDefaultAsync(n => n.Id == novelId);
            if (novel is null) return NotFound();
            Novel = novel;
            await LoadTree(novelId);
            return Page();
        }

        private async Task LoadTree(int novelId)
        {
            Tree = await _context.Volumes
                .Where(v => v.NovelId == novelId)
                .OrderBy(v => v.VolumeNumber)
                .Select(v => new VolumeTreeDto
                {
                    Id = v.Id,
                    Title = v.Title,
                    VolumeNumber = v.VolumeNumber,
                    Type = v.Type,
                    PdfUrl = v.PdfUrl,
                    CoverUrl = v.CoverUrl,
                    Description = v.Description,
                    Chapters = v.Chapters
                        .OrderBy(c => c.ChapterNumber)
                        .Select(c => new ChapterTreeDto
                        {
                            Id = c.Id,
                            Title = c.Title,
                            Slug = c.Slug,
                            ChapterNumber = c.ChapterNumber
                        }).ToList()
                }).ToListAsync();
        }

        // ══════════════════════════════════════════════════════════
        // VOLUME handlers
        // ══════════════════════════════════════════════════════════

        // Create Volume
        public async Task<IActionResult> OnPostCreateVolumeAsync(
            int novelId, string title, int volumeNumber,
            VolumeType type, string? description, string? coverUrl, string? pdfUrl)
        {
            if (!await OwnsNovel(novelId)) return Forbid();
            if (string.IsNullOrWhiteSpace(title))
                return BadRequest("Title required.");

            _context.Volumes.Add(new Volume
            {
                NovelId = novelId,
                Title = title,
                VolumeNumber = volumeNumber,
                Type = type,
                Description = description,
                CoverUrl = coverUrl,
                PdfUrl = pdfUrl,
                CreatedAt = DateTime.Now
            });
            await _context.SaveChangesAsync();
            return RedirectToPage(new { novelId });
        }

        // Edit Volume
        public async Task<IActionResult> OnPostEditVolumeAsync(
            int novelId, int volumeId, string title, int volumeNumber,
            VolumeType type, string? description, string? coverUrl, string? pdfUrl)
        {
            if (!await OwnsNovel(novelId)) return Forbid();
            var vol = await _context.Volumes.FindAsync(volumeId);
            if (vol is null || vol.NovelId != novelId) return NotFound();

            vol.Title = title;
            vol.VolumeNumber = volumeNumber;
            vol.Type = type;
            vol.Description = description;
            vol.CoverUrl = coverUrl;
            vol.PdfUrl = pdfUrl;
            vol.LastUpdatedAt = DateTime.Now;
            await _context.SaveChangesAsync();
            return RedirectToPage(new { novelId });
        }

        // Delete Volume
        public async Task<IActionResult> OnPostDeleteVolumeAsync(int novelId, int volumeId)
        {
            if (!await OwnsNovel(novelId)) return Forbid();
            var vol = await _context.Volumes
                .Include(v => v.Chapters)
                .FirstOrDefaultAsync(v => v.Id == volumeId && v.NovelId == novelId);
            if (vol is null) return NotFound();
            _context.Chapters.RemoveRange(vol.Chapters);
            _context.Volumes.Remove(vol);
            await _context.SaveChangesAsync();
            return RedirectToPage(new { novelId });
        }

        // ══════════════════════════════════════════════════════════
        // CHAPTER handlers
        // ══════════════════════════════════════════════════════════

        // Create Chapter
        public async Task<IActionResult> OnPostCreateChapterAsync(
            int novelId, int volumeId, string title, decimal chapterNumber, string? content)
        {
            if (!await OwnsNovel(novelId)) return Forbid();
            if (string.IsNullOrWhiteSpace(title)) return BadRequest("Title required.");
            // Kiểm tra trùng ChapterNumber trong cùng Volume
            var isDuplicate = await _context.Chapters
                .AnyAsync(c => c.NovelId == novelId
                            && c.VolumeId == volumeId
                            && c.ChapterNumber == chapterNumber);

            if (isDuplicate)
                return new JsonResult(new
                {
                    success = false,
                    message = $"Chapter {chapterNumber} already exists in this volume."
                })
                { StatusCode = 409 };

            var wordCount = string.IsNullOrEmpty(content)
                ? 0
                : content.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length;

            _context.Chapters.Add(new Chapter
            {
                NovelId = novelId,
                VolumeId = volumeId,
                Title = title,
                ChapterNumber = chapterNumber,
                Slug = GenerateSlug(title),
                Content = content,
                WordCount = wordCount,
                CreatedAt = DateTime.Now
            });
            await _context.SaveChangesAsync();
            return RedirectToPage(new { novelId });
        }

        // Edit Chapter
        public async Task<IActionResult> OnPostEditChapterAsync(
            int novelId, int chapterId, string title, decimal chapterNumber, string? content)
        {
            if (!await OwnsNovel(novelId)) return Forbid();
            var ch = await _context.Chapters.FindAsync(chapterId);
            if (ch is null || ch.NovelId != novelId) return NotFound();
            // Kiểm tra trùng ChapterNumber — bỏ qua chính nó
            var isDuplicate = await _context.Chapters
                .AnyAsync(c => c.NovelId == novelId
                            && c.VolumeId == ch.VolumeId
                            && c.ChapterNumber == chapterNumber
                            && c.Id != chapterId);

            if (isDuplicate)
                return new JsonResult(new
                {
                    success = false,
                    message = $"Chapter {chapterNumber} already exists in this volume."
                })
                { StatusCode = 409 };
            ch.Title = title;
            ch.ChapterNumber = chapterNumber;
            ch.Content = content;
            ch.WordCount = string.IsNullOrEmpty(content)
                ? 0 : content.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length;
            ch.LastUpdatedAt = DateTime.Now;
            await _context.SaveChangesAsync();
            return RedirectToPage(new { novelId });
        }

        // Delete Chapter
        public async Task<IActionResult> OnPostDeleteChapterAsync(int novelId, int chapterId)
        {
            if (!await OwnsNovel(novelId)) return Forbid();
            var ch = await _context.Chapters
                .FirstOrDefaultAsync(c => c.Id == chapterId && c.NovelId == novelId);
            if (ch is null) return NotFound();
            _context.Chapters.Remove(ch);
            await _context.SaveChangesAsync();
            return RedirectToPage(new { novelId });
        }

        // Thêm vào ManageModel
        public async Task<IActionResult> OnGetChapterContentAsync(int chapterId)
        {
            var ch = await _context.Chapters
                .FirstOrDefaultAsync(c => c.Id == chapterId);
            if (ch is null) return NotFound();
            return new JsonResult(new { content = ch.Content ?? "" });
        }

        // ── Helpers ───────────────────────────────────────────────
        private async Task<bool> OwnsNovel(int novelId)
        {
            var novel = await _context.Novels.FindAsync(novelId);
            if (novel is null) return false;
            var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
            return novel.PublisherId == userId || User.IsInRole("Admin");
        }

        private static string GenerateSlug(string title)
        {
            var slug = title.ToLower().Trim();
            slug = System.Text.RegularExpressions.Regex.Replace(slug, @"[^a-z0-9\s-]", "");
            slug = System.Text.RegularExpressions.Regex.Replace(slug, @"\s+", "-").Trim('-');
            return $"{slug}-{Guid.NewGuid():N}"[..^26];
        }
    }


}