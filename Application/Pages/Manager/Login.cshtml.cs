using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Application.Pages.Manager;

public class Login : PageModel
{
    [BindProperty] public string Email { get; set; }

    [BindProperty] public string Password { get; set; }

    [TempData] public string? ErrorMessage { get; set; }

    public IActionResult OnPost()
    {
        Console.WriteLine(Email);
        Console.WriteLine(Password);

        if (Email == "truong@gmail.com" && Password == "123123")
        {
            return RedirectToPage("/index");
        }

        ErrorMessage = "Invalid email or password.";

        return Page();
    }

    public async Task OnGetAsync()
    {
    }
}