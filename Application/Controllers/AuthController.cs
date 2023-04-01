using Microsoft.AspNetCore.Mvc;
using Service.Interfaces;
using Service.Models.Auth;
using Service.Models.Chat;
using Shared.Helpers;
using Swashbuckle.AspNetCore.Annotations;

namespace Application.Controllers;

[Route("auth")]
public class AuthController : ApiController
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    // [SwaggerOperation(Description = "Phone format: '84' or '+84'. Test: 84389376290/123123")]
    [HttpPost("")]
    [ProducesResponseType(typeof(ChatTokenResponseModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Authenticate(LoginModel model)
    {
        var result = await _authService.Authenticate(model);
        return result.Match(Ok, OnError);
    }
}