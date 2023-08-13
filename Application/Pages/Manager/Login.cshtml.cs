using Data.EFCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
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

    public async Task<IActionResult> OnPost()
    {
        var user = await _unitOfWork.Managers.Query()
            .Where(e => e.Email == Email).FirstOrDefaultAsync();

        if (user != null && user.Password == AuthHelper.HashPassword(Password))
            return RedirectToPage("Tours");
        
        return Page();
    }

    public void OnGet()
    {
    }
}