using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using NovelManangment.Data;
using NovelManangment.Dtos.Comments;
using NovelManangment.Dtos.Novels;
using NovelManangment.Models;
using System.Security.Claims;

namespace NovelManangment.Pages.Novels
{
    public class DetailModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        public DetailModel(ApplicationDbContext context) => _context = context;

        public Novel Novel { get; set; } = null!;
        public List<VolumeWithChaptersDto> Volumes { get; set; } = new();

        //Follow
        public bool IsFollowing { get; set; }
        public int FollowCount { get; set; }

        //Rating
        public int? UserRating { get; set; }
        public double AverageRating { get; set; }
        public int RatingCount { get; set; }

        //Comment
        public List<CommentDto> Comments { get; set; } = new();
        public int CommentPage { get; set; } = 1;
        public int CommentTotalPages { get; set; }
        public int CommentTotal { get; set; }
        private const int CommentPageSize = 10;

        public async Task<IActionResult> OnGetAsync(string slug)
        {
            var novel = await _context.Novels
                .Include(n => n.Publisher)
                .Include(n => n.Genres)
                .FirstOrDefaultAsync(n => n.Slug == slug);


            if (novel is null) return NotFound();


            if (novel.PublishStatus != PublishStatus.Approved 
                && (User.IsInRole(UserRole.Member.ToString()) || User.Identity?.IsAuthenticated != true))
            {
                return NotFound();
            }
            Novel = novel;

            Volumes = await _context.Volumes
                .Where(v => v.NovelId == novel.Id)
                .OrderBy(v => v.VolumeNumber)
                .Select(v => new VolumeWithChaptersDto
                {
                    Id = v.Id,
                    Title = v.Title,
                    CoverUrl = v.CoverUrl,
                    PdfUrl = v.PdfUrl,
                    Type = v.Type,
                    Chapters = v.Chapters
                        .OrderBy(c => c.ChapterNumber)
                        .Select(c => new ChapterRowDto
                        {
                            Slug = c.Slug,
                            Title = c.Title,
                            ChapterNumber = c.ChapterNumber,
                            CreatedAt = c.CreatedAt
                        })
                        .ToList()
                })
                .ToListAsync();

            //Follow
            FollowCount = await _context.Follows
                .CountAsync(f => f.NovelId == novel.Id);
            //Rating count
            RatingCount = await _context.NovelRatings
                .CountAsync(r => r.NovelId == novel.Id);
            AverageRating = RatingCount > 0 
                ? await _context.NovelRatings
                    .Where(r => r.NovelId == novel.Id)
                    .AverageAsync(r => (double)r.Score)
                : 0;

            if (HttpContext.User.Identity?.IsAuthenticated == true)
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
                IsFollowing = await _context.Follows
                    .AnyAsync(f => f.NovelId == novel.Id && f.UserId == userId);
                var rating = await _context.NovelRatings
                    .FirstOrDefaultAsync(r => r.NovelId == novel.Id && r.UserId == userId);
                UserRating = rating?.Score;
            }

            //Comment
            CommentPage = 10;
            CommentTotal = await _context.Comments
                .CountAsync(c => c.NovelId == novel.Id);
            CommentTotalPages = (int)Math.Ceiling(CommentTotal / (double)CommentPageSize);

            Comments = await _context.Comments
                .Where(c => c.NovelId == novel.Id)
                .OrderByDescending(c => c.CreatedAt)
                .Skip(0).Take(CommentPageSize)
                .Select(c => new CommentDto
                {
                    Id = c.Id,
                    UserId = c.UserId,
                    AvatarUrl = c.User.AvatarUrl,
                    DisplayName = c.User.DisplayName,
                    Content = c.Content,
                    CreatedAt = c.CreatedAt,
                    ChapterTitle = c.ChapterId != null
                        ? c.Chapter!.Title
                        : null
                    ,
                    ChapterSlug = c.ChapterId != null
                        ? c.Chapter!.Slug
                        : null
                })
                .ToListAsync();

            return Page();
        }

        // ── Toggle Follow ──
        //[Authorize]
        public async Task<IActionResult> OnPostToggleFollowAsync(string slug)
        {
            if (!User.Identity?.IsAuthenticated ?? true)
            {
                return Unauthorized();
            }
            var novel = await _context.Novels.FirstOrDefaultAsync(n => n.Slug == slug);
            if (novel is null) return NotFound();

            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var existing = await _context.Follows
                .FirstOrDefaultAsync(f => f.NovelId == novel.Id && f.UserId == userId);

            if (existing is null)
            {
                _context.Follows.Add(new NovelFollow
                {
                    UserId = userId,
                    NovelId = novel.Id,
                    CreatedAt = DateTime.Now
                });
            }
            else
            {
                _context.Follows.Remove(existing);
            }

            await _context.SaveChangesAsync();

            var followCount = await _context.Follows.CountAsync(f => f.NovelId == novel.Id);
            return new JsonResult(new
            {
                following = existing is null, // null = vừa thêm = đang follow
                followCount = followCount
            });
        }

        // ── Rate ──
        //[Authorize]
        public async Task<IActionResult> OnPostRateAsync(string slug, int score)
        {
            if (!User.Identity?.IsAuthenticated ?? true)
            {
                return Unauthorized();
            }
            if (score < 1 || score > 5)
                return BadRequest("Score must be between 1 and 5.");

            var novel = await _context.Novels.FirstOrDefaultAsync(n => n.Slug == slug);
            if (novel is null) return NotFound();

            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var existing = await _context.NovelRatings
                .FirstOrDefaultAsync(r => r.NovelId == novel.Id && r.UserId == userId);

            if (existing is null)
            {
                _context.NovelRatings.Add(new NovelRating
                {
                    UserId = userId,
                    NovelId = novel.Id,
                    Score = score,
                    CreatedAt = DateTime.Now
                });
            }
            else
            {
                existing.Score = score;
                existing.UpdatedAt = DateTime.Now;
            }

            await _context.SaveChangesAsync();

            var ratingCount = await _context.NovelRatings.CountAsync(r => r.NovelId == novel.Id);
            var averageRating = await _context.NovelRatings
                .Where(r => r.NovelId == novel.Id)
                .AverageAsync(r => (double)r.Score);

            return new JsonResult(new
            {
                userRating = score,
                averageRating = Math.Round(averageRating, 1),
                ratingCount = ratingCount
            });
        }

        public async Task<IActionResult> OnGetCommentsAsync(string slug, int pageNumber = 1)
        {
            var novel = await _context.Novels.FirstOrDefaultAsync(n => n.Slug == slug);
            if (novel is null) return NotFound();

            const int size = 1;
            var total = await _context.Comments.CountAsync(c => c.NovelId == novel.Id);
            var totalPages = (int)Math.Ceiling(total / (double)size);

            var comments = await _context.Comments
                .Where(c => c.NovelId == novel.Id)
                .OrderByDescending(c => c.CreatedAt)
                .Skip((pageNumber - 1) * size).Take(size)
                .Select(c => new CommentDto
                {
                    Id = c.Id,
                    UserId = c.UserId,
                    AvatarUrl = c.User.AvatarUrl,
                    DisplayName = c.User.DisplayName,
                    Content = c.Content,
                    CreatedAt = c.CreatedAt,
                    ChapterTitle = c.ChapterId != null
                            ? c.Chapter!.Title
                            : null
                        ,
                    ChapterSlug = c.ChapterId != null
                            ? c.Chapter!.Slug
                            : null
                })
                .ToListAsync();

            return new JsonResult(new { comments, total, totalPages, page = pageNumber});
        }

        public async Task<IActionResult> OnPostCommentAsync(string slug, string content)
        {
            if (!User.Identity?.IsAuthenticated ?? true)
                return new JsonResult(new { error = "Unauthorized" }) { StatusCode = 401 };

            if (string.IsNullOrWhiteSpace(content) || content.Length > 2000)
                return new JsonResult(new { error = "Invalid content" }) { StatusCode = 400 };

            var novel = await _context.Novels.FirstOrDefaultAsync(n => n.Slug == slug);
            if (novel is null) return NotFound();

            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var userName = User.FindFirst(ClaimTypes.Name)?.Value ?? "User";

            var comment = new Comment
            {
                UserId = userId,
                NovelId = novel.Id,
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

        public string GetTimeAgo(DateTime dt)
        {
            var s = DateTime.Now - dt;
            if (s.Days > 365) return $"{s.Days / 365}y ago";
            if (s.Days > 30) return $"{s.Days / 30}mo ago";
            if (s.Days > 0) return $"{s.Days}d ago";
            if (s.Hours > 0) return $"{s.Hours}h ago";
            if (s.Minutes > 0) return $"{s.Minutes}m ago";
            return "Just now";
        }
    }

}