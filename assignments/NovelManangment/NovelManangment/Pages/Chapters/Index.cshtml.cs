using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using NovelManangment.Data;
using NovelManangment.Dtos.Comments;
using NovelManangment.Models;
using System.Security.Claims;

namespace NovelManangment.Pages.Chapters
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        public IndexModel(ApplicationDbContext context) => _context = context;

        public Chapter Chapter { get; set; } = null!;
        public Novel Novel { get; set; } = null!;
        public Volume Volume { get; set; } = null!;
        public Chapter? PrevChapter { get; set; }
        public Chapter? NextChapter { get; set; }

        //Comments
        public List<ChapterCommentDto> Comments { get; set; } = new();
        public int CommentTotalPages { get; set; }
        public int CommentTotal { get; set; }
        private const int CommentPageSize = 10;

        public async Task<IActionResult> OnGetAsync(string slug, string chSlug)
        {
            var chapter = await _context.Chapters
                .Include(c => c.Novel)//.ThenInclude(n => n.Genres)
                .Include(c => c.Volume)
                .FirstOrDefaultAsync(c => c.Novel.Slug == slug && c.Slug == chSlug);

            if (chapter is null) return NotFound();

            Chapter = chapter;
            Novel = chapter.Novel;
            Volume = chapter.Volume;

            //// Tăng view count
            chapter.ViewCount++;
            await _context.SaveChangesAsync();

            // Prev / Next trong cùng novel
            PrevChapter = await _context.Chapters
                .Where(c => c.NovelId == Novel.Id)
                .Where(c => c.Volume.VolumeNumber < chapter.Volume.VolumeNumber
                         || (c.Volume.VolumeNumber == chapter.Volume.VolumeNumber
                             && c.ChapterNumber < chapter.ChapterNumber))
                .OrderByDescending(c => c.Volume.VolumeNumber)
                .ThenByDescending(c => c.ChapterNumber)
                .FirstOrDefaultAsync();

            NextChapter = await _context.Chapters
                .Where(c => c.NovelId == Novel.Id)
                .Where(c => c.Volume.VolumeNumber > chapter.Volume.VolumeNumber
                         || (c.Volume.VolumeNumber == chapter.Volume.VolumeNumber
                             && c.ChapterNumber > chapter.ChapterNumber))
                .OrderBy(c => c.Volume.VolumeNumber)
                .ThenBy(c => c.ChapterNumber)
                .FirstOrDefaultAsync();

            //Comment
            CommentTotal = await _context.Comments.CountAsync(c => c.ChapterId == chapter.Id);
            CommentTotalPages = (int)Math.Ceiling(CommentTotal / (double)CommentPageSize);

            Comments = await _context.Comments
                .Where(c => c.ChapterId == chapter.Id)
                .OrderByDescending(c => c.CreatedAt)
                .Skip(0).Take(CommentPageSize)
                .Select(c => new ChapterCommentDto 
                { 
                    Id = c.Id, 
                    UserId = c.UserId,
                    AvatarUrl = c.User.AvatarUrl,
                    DisplayName = c.User.DisplayName, 
                    Content = c.Content, 
                    CreatedAt = c.CreatedAt 
                })
                .ToListAsync();

            return Page();
        }

        public async Task<IActionResult> OnGetCommentsAsync(string slug, string chSlug, int pageNumber = 1)
        {
            var chapter = await _context.Chapters
                .Include(c => c.Novel)
                .FirstOrDefaultAsync(c => c.Novel.Slug == slug && c.Slug == chSlug);
            if (chapter is null) return NotFound();

            const int size = 10;
            var total = await _context.Comments.CountAsync(c => c.ChapterId == chapter.Id);
            var totalPages = (int)Math.Ceiling(total / (double)size);

            var comments = await _context.Comments
                .Where(c => c.ChapterId == chapter.Id)
                .OrderByDescending(c => c.CreatedAt)
                .Skip((pageNumber - 1) * size).Take(size)
                .Select(c => new ChapterCommentDto
                {
                    Id = c.Id,
                    UserId = c.UserId,
                    AvatarUrl = c.User.AvatarUrl,
                    DisplayName = c.User.DisplayName,
                    Content = c.Content,
                    CreatedAt = c.CreatedAt
                })
                .ToListAsync();

            return new JsonResult(new { comments, total, totalPages, page = pageNumber });
        }

        public async Task<IActionResult> OnPostCommentAsync(string slug, string chSlug, string content)
        {
            if (!User.Identity?.IsAuthenticated ?? true)
                return new JsonResult(new { error = "Unauthorized" }) { StatusCode = 401 };

            if (string.IsNullOrWhiteSpace(content) || content.Length > 2000)
                return new JsonResult(new { error = "Invalid content" }) { StatusCode = 400 };

            var chapter = await _context.Chapters
                .Include(c => c.Novel)
                .FirstOrDefaultAsync(c => c.Novel.Slug == slug && c.Slug == chSlug);
            if (chapter is null) return NotFound();

            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var userName = User.FindFirst(ClaimTypes.Name)?.Value ?? "User";

            var comment = new Comment
            {
                UserId = userId,
                NovelId = chapter.NovelId,
                ChapterId = chapter.Id,
                Content = content.Trim(),
                CreatedAt = DateTime.Now
            };
            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();

            return new JsonResult(new
            {
                id = comment.Id,
                userName,
                content = comment.Content,
                createdAt = comment.CreatedAt.ToString("dd MMM yyyy HH:mm")
            });
        }
    }
}