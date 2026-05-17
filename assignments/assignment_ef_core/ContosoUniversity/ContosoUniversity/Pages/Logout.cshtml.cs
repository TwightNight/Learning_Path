using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ContosoUniversity.Pages
{
    public class LogoutModel : PageModel
    {
        public IActionResult OnGet()
        {
            // Xóa Cookie chứa JWT
            Response.Cookies.Delete("X-Access-Token");
            return RedirectToPage("/Login");
        }
    }
}