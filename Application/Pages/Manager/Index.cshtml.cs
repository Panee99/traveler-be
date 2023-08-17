using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Application.Pages.Manager;

public class IndexModel : PageModel
{
    public IActionResult OnGetLogout()
    {
        HttpContext.Session.Remove("User");
        return RedirectToPage("Login");
    }
    
    public void OnGet()
    {
    }
}