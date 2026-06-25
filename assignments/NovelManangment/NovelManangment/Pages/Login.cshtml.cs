using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using NovelManangment.Data;
using NovelManangment.Dtos.Auth;
using NovelManangment.Services;
using System.ComponentModel.DataAnnotations;

namespace NovelManangment.Pages
{
    public class LoginModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly TokenService _tokenService;

        public LoginModel(ApplicationDbContext context, TokenService tokenService)
        {
            _context = context;
            _tokenService = tokenService;
        }

        [BindProperty]
        public LoginDto Input { get; set; } = new();

        public string? ErrorMessage { get; set; }

        public IActionResult OnGet()
        {
            // Nếu đã đăng nhập thì redirect thẳng vào dashboard
            if (Request.Cookies.ContainsKey("X-Access-Token"))
                return RedirectToPage("/Index");
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) return Page();
            Response.Cookies.Delete("X-Access-Token");
            var user = await _context.Users
                .FirstOrDefaultAsync(u => 
                    u.UserName == Input.UserName &&
                    u.Password == Input.Password &&
                    !u.IsDeleted);

            if (user is null)
            {
                ErrorMessage = "Invalid username or password.";
                return Page();
            }

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