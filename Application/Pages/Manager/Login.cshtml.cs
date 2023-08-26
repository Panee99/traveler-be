using Application.Commons;
using Data.EFCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Shared.Helpers;

namespace Application.Pages.Manager;

[IgnoreAntiforgeryToken]
public class LoginModel : PageModel
{
    private readonly UnitOfWork _unitOfWork;

    public LoginModel(UnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    [BindProperty] public string Email { get; set; } = null!;

    [BindProperty] public string Password { get; set; } = null!;

    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Login
    /// </summary>
    public async Task<IActionResult> OnPost()
    {
        // Find user
        var user = await _unitOfWork.Managers.Query()
            .Where(e => e.Email == Email)
            .FirstOrDefaultAsync();

        // Check if user password valid
        if (user is null || user.Password != AuthHelper.HashPassword(Password))
        {
            ErrorMessage = "Login failed, wrong email or password !";
            return Page();
        }

        // Add User to session
        HttpContext.Session.SetString("User", JsonConvert.SerializeObject(new UserSessionModel()
        {
            Id = user.Id,
            Role = user.Role,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName
        }));

        return RedirectToPage("Tours");
    }

    /// <summary>
    /// Get page
    /// </summary>
    public IActionResult OnGet()
    {
        if (RazorPageHelper.GetUserFromSession(HttpContext.Session) is null)
            return Page();

        // Redirect to Home if already logged in
        return RedirectToPage("Tours");
    }
}