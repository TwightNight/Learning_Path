using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using NovelManangment.Data;
using NovelManangment.Dtos.Users;
using NovelManangment.Models;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace NovelManangment.Pages.Users
{
    [Authorize]
    public class EditModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        public EditModel(ApplicationDbContext context) => _context = context;

        public User User { get; set; } = null!;
        public int NovelCount { get; set; }
        public bool CanChangeRole { get; set; }

        [BindProperty] public UserEditDto Input { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(int userId)
        {
            var currentUserId = int.Parse(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var isAdmin = HttpContext.User.IsInRole("Admin");

            if (!isAdmin && currentUserId != userId)
                return Forbid();

            var user = await _context.Users
                .Include(u => u.Novels)
                .FirstOrDefaultAsync(u => u.Id == userId);
            if (user is null) return NotFound();

            CanChangeRole = isAdmin;

            User = user;
            NovelCount = user.Novels.Count;
            Input = new UserEditDto
            {
                UserId = user.Id,
                DisplayName = user.DisplayName,
                Email = user.Email,
                Role = user.Role,
                AvatarUrl = user.AvatarUrl
            };
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var currentUserId = int.Parse(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var isAdmin = HttpContext.User.IsInRole("Admin");

            if (!isAdmin && currentUserId != Input.UserId)
                return Forbid();

            if (!ModelState.IsValid)
            {
                User = (await _context.Users.FindAsync(Input.UserId))!;
                CanChangeRole = isAdmin;
                return Page();
            }

            // Check duplicate email (exclude self)
            if (await _context.Users.AnyAsync(u =>
                    u.Email == Input.Email && u.Id != Input.UserId))
            {
                ModelState.AddModelError("Input.Email", "Email already in use.");
                User = (await _context.Users.FindAsync(Input.UserId))!;
                CanChangeRole = isAdmin;
                return Page();
            }

            var user = await _context.Users.FindAsync(Input.UserId);
            if (user is null) return NotFound();

            user.DisplayName = Input.DisplayName;
            user.Email = Input.Email;
            //user.Role = Input.Role;
            user.AvatarUrl = Input.AvatarUrl;
            user.LastUpdatedAt = DateTime.Now;

            if (isAdmin)
                user.Role = Input.Role;
            // Reset password nếu có nhập
            if (!string.IsNullOrWhiteSpace(Input.NewPassword))
            {
                if (Input.NewPassword.Length < 6)
                {
                    ModelState.AddModelError("Input.NewPassword",
                        "Password must be at least 6 characters.");
                    User = user;
                    CanChangeRole = isAdmin;
                    return Page();
                }
                user.Password = Input.NewPassword;
            }

            await _context.SaveChangesAsync();
            TempData["Success"] = $"User \"{user.DisplayName}\" updated.";
            return isAdmin
                ? RedirectToPage("/Users/Index")
                : RedirectToPage("/Users/Detail", new { userId = user.Id });
        }
    }

}