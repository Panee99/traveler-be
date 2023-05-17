using Microsoft.AspNetCore.Mvc;
using Service.Interfaces;
using Service.Models.Auth;
using Service.Models.Chat;
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

    [SwaggerOperation(Description = "traveler:84389376290 - admin@gmail.com - guide@gmail.com - Pass:123123")]
    [ProducesResponseType(typeof(ChatTokenResponseModel), StatusCodes.Status200OK)]
    [HttpPost("")]
    public async Task<IActionResult> Authenticate(LoginModel model)
    {
        var result = await _authService.Authenticate(model);
        return result.Match(Ok, OnError);
    }
}