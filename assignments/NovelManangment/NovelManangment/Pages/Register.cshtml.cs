using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using NovelManangment.Data;
using NovelManangment.Dtos.Auth;
using NovelManangment.Models;
using NovelManangment.Services;
using System.ComponentModel.DataAnnotations;

namespace NovelManangment.Pages
{
    public class RegisterModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly TokenService _tokenService;

        public RegisterModel(ApplicationDbContext context, TokenService tokenService)
        {
            _context = context;
            _tokenService = tokenService;
        }

        [BindProperty]
        public RegisterDto Input { get; set; } = new();

        public string? ErrorMessage { get; set; }

        public IActionResult OnGet()
        {
            if (Request.Cookies.ContainsKey("X-Access-Token"))
                return RedirectToPage("/Index");
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) return Page();

            // Kiểm tra trùng username / email
            if (await _context.Users.AnyAsync(u => u.UserName == Input.UserName))
            {
                ErrorMessage = "Username already taken.";
                return Page();
            }
            if (await _context.Users.AnyAsync(u => u.Email == Input.Email))
            {
                ErrorMessage = "Email already registered.";
                return Page();
            }

            var user = new User
            {
                UserName = Input.UserName,
                Email = Input.Email,
                DisplayName = Input.DisplayName,
                Password = Input.Password,
                CreatedAt = DateTime.Now
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Đăng nhập luôn sau khi đăng ký
            var token = _tokenService.CreateToken(user);
            Response.Cookies.Append("X-Access-Token", token, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTimeOffset.UtcNow.AddDays(7)
            });

            return RedirectToPage("/Index");
        }
    }

}