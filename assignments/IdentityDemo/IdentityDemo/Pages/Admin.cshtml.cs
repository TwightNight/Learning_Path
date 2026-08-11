using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace IdentityDemo.Pages;

[Authorize(Roles = "Admin")] // Nếu chưa đăng nhập -> redirect tới LoginPath; nếu không đúng role -> AccessDeniedPath
public class AdminModel : PageModel
{
    public void OnGet()
    {
    }
}
