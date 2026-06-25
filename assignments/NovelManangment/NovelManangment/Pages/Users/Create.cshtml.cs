using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using NovelManangment.Data;
using NovelManangment.Dtos.Users;
using NovelManangment.Models;
using System.ComponentModel.DataAnnotations;

namespace NovelManangment.Pages.Users
{
    [Authorize(Roles = "Admin")]
    public class CreateModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        public CreateModel(ApplicationDbContext context) => _context = context;

        [BindProperty] public UserCreateDto Input { get; set; } = new();

        public void OnGet() { }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) return Page();

            if (await _context.Users.AnyAsync(u => u.UserName == Input.UserName))
            {
                ModelState.AddModelError("Input.UserName", "Username already exists.");
                return Page();
            }
            if (await _context.Users.AnyAsync(u => u.Email == Input.Email))
            {
                ModelState.AddModelError("Input.Email", "Email already exists.");
                return Page();
            }

            _context.Users.Add(new User
            {
                UserName = Input.UserName,
                Email = Input.Email,
                Password = Input.Password,
                DisplayName = Input.DisplayName,
                Role = Input.Role,
                AvatarUrl = Input.AvatarUrl,
                CreatedAt = DateTime.Now,
                IsDeleted = false
            });
            await _context.SaveChangesAsync();
            TempData["Success"] = $"User \"{Input.DisplayName}\" created successfully.";
            return RedirectToPage("/Users/Index");
        }

    }

 
}