using Microsoft.AspNetCore.Mvc;
using Service.Interfaces;
using Service.Models.Auth;
using Service.Models.Chat;

namespace Application.Controllers
{
    [Route("auth")]
    public class AuthController : ApiController
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("traveler")]
        [ProducesResponseType(typeof(ChatTokenResponseModel), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public IActionResult AuthenticateTraveler(PhoneLoginModel model)
        {
            var result = _authService.AuthenticateTraveler(model);
            return result.Match(Ok, OnError);
        }

        [HttpPost("manager")]
        [ProducesResponseType(typeof(ChatTokenResponseModel), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public IActionResult AuthenticateManager(EmailLoginModel model)
        {
            var result = _authService.AuthenticateManager(model);
            return result.Match(Ok, OnError);
        }

        [HttpPost("tour-guide")]
        [ProducesResponseType(typeof(ChatTokenResponseModel), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public IActionResult AuthenticateTourGuide(EmailLoginModel model)
        {
            var result = _authService.AuthenticateTourGuide(model);
            return result.Match(Ok, OnError);
        }
    }
}