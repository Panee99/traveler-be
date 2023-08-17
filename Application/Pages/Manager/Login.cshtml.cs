using Data.EFCore;
using Data.Entities;
using Data.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Shared.Helpers;

namespace Application.Pages.Manager;

public class Login : PageModel
{
    private readonly UnitOfWork _unitOfWork;

    public Login(UnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    [BindProperty] public string Email { get; set; } = null!;

    [BindProperty] public string Password { get; set; } = null!;

    public string? ErrorMessage { get; set; }

    public async Task<IActionResult> OnPost()
    {
        var user = await _unitOfWork.Managers.Query()
            .Where(e => e.Email == Email)
            .FirstOrDefaultAsync();

        if (user is null || user.Password != AuthHelper.HashPassword(Password))
        {
            ErrorMessage = "Login failed, wrong email or password !";
            return Page();
        }

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

    public IActionResult OnGet()
    {
        // Authenticate User
        var userSessionData = HttpContext.Session.GetString("User");
        var userData = userSessionData is null
            ? null
            : JsonConvert.DeserializeObject<UserSessionModel>(userSessionData);
        if (userData is null)
            return Page();
        
        return RedirectToPage("Tours");
    }
}

public record UserSessionModel
{
    public Guid Id;
    public UserRole Role;
    public string Email = null!;
    public string FirstName = null!;
    public string LastName = null!;
}