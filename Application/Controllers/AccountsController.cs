using Application.Configurations.Auth;
using Microsoft.AspNetCore.Mvc;
using Service.Interfaces;
using Service.Models.Account;

namespace Application.Controllers;

[Authorize]
[Route("accounts")]
public class AccountsController : ApiController
{
    private readonly IAccountService _accountService;

    public AccountsController(IAccountService accountService)
    {
        _accountService = accountService;
    }

    [ProducesResponseType(typeof(AccountViewModel), StatusCodes.Status200OK)]
    [HttpGet("current/profile")]
    public async Task<IActionResult> GetProfile()
    {
        var result = await _accountService.GetProfile(CurrentUser.Id, CurrentUser.Role);
        return result.Match(Ok, OnError);
    }

    [ProducesResponseType(typeof(AccountViewModel), StatusCodes.Status200OK)]
    [HttpPatch("current/profile")]
    public async Task<IActionResult> UpdateProfile(ProfileUpdateModel model)
    {
        var result = await _accountService.UpdateProfile(CurrentUser.Id, model);
        return result.Match(Ok, OnError);
    }
}