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

    [SwaggerOperation(Description = "Phone format: '84' or '+84'. Test: 84389376290/123123")]
    [HttpPost("traveler")]
    [ProducesResponseType(typeof(ChatTokenResponseModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public IActionResult AuthenticateTraveler(PhoneLoginModel model)
    {
        if (model.Phone.StartsWith('+')) model.Phone = model.Phone.Substring(1);
        var result = _authService.AuthenticateTraveler(model);
        return result.Match(Ok, OnError);
    }

    [SwaggerOperation(Description = "manager@gmail.com / 123123")]
    [HttpPost("manager")]
    [ProducesResponseType(typeof(ChatTokenResponseModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public IActionResult AuthenticateManager(EmailLoginModel model)
    {
        var result = _authService.AuthenticateManager(model);
        return result.Match(Ok, OnError);
    }

    [SwaggerOperation(Description = "guide@gmail.com / 123123")]
    [HttpPost("tour-guide")]
    [ProducesResponseType(typeof(ChatTokenResponseModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public IActionResult AuthenticateTourGuide(EmailLoginModel model)
    {
        var result = _authService.AuthenticateTourGuide(model);
        return result.Match(Ok, OnError);
    }
}