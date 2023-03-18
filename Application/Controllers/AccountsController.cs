using Application.Configurations.Auth;
using Microsoft.AspNetCore.Mvc;
using Service.Interfaces;
using Service.Models.Attachment;
using Shared;
using Swashbuckle.AspNetCore.Annotations;

namespace Application.Controllers;

[Route("accounts")]
public class AccountsController : ApiController
{
    private readonly IAccountService _accountService;

    public AccountsController(IAccountService accountService)
    {
        _accountService = accountService;
    }

    [SwaggerOperation(description: "File size < 5MB")]
    [Authorize]
    [ProducesResponseType(typeof(AttachmentViewModel), StatusCodes.Status200OK)]
    [HttpPut("avatar")]
    public async Task<IActionResult> UpdateAvatar(IFormFile file)
    {
        if (file.Length > AppConstants.FileSizeMax)
            return BadRequest("File too big, max = 5mb.");

        var result = await _accountService.UpdateAvatar(CurrentUser.Id, file.ContentType, file.OpenReadStream());
        return result.Match(Ok, OnError);
    }
}