using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace IdentityDemo.Pages;

public class IndexModel : PageModel
{
    private readonly UserManager<IdentityUser> _userManager;

    public IndexModel(UserManager<IdentityUser> userManager)
    {
        _userManager = userManager;
    }

    public IList<string> Roles { get; set; } = new List<string>();

    public async Task OnGetAsync()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            var user = await _userManager.GetUserAsync(User); // Lấy IdentityUser từ ClaimsPrincipal hiện tại
            if (user is not null)
            {
                Roles = await _userManager.GetRolesAsync(user);
            }
        }
    }
}
