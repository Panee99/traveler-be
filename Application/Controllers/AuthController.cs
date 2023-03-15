using Application.Configurations.Middleware;
using Microsoft.AspNetCore.Mvc;
using Service.Interfaces;
using System.ComponentModel.DataAnnotations;
using Service.Models.Account;
using Service.Models.Auth;

namespace Application.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [Route("managers")]
        [HttpPost]
        [ProducesResponseType(typeof(AuthViewModel), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<AuthViewModel>> AuthenticateManager([FromBody][Required] AuthRequestModel model)
        {
            var manaer = await _authService.AuthenticateManager(model);
            if (manaer is null)
            {
                return NotFound();
            }
            return Ok(manaer);
        }

        [Route("tour-guides")]
        [HttpPost]
        [ProducesResponseType(typeof(AuthViewModel), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<AuthViewModel>> AuthenticateTourGuide([FromBody][Required] AuthRequestModel model)
        {
            var tourGuide = await _authService.AuthenticateTourGuide(model);
            if (tourGuide is null)
            {
                return NotFound();
            }
            return Ok(tourGuide);
        }

        [Route("travelers")]
        [HttpPost]
        [ProducesResponseType(typeof(AuthViewModel), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<AuthViewModel>> AuthenticateTraveler([FromBody][Required] AuthRequestModel model)
        {
            var traveler = await _authService.AuthenticateTraveler(model);
            if (traveler is null)
            {
                return NotFound();
            }
            return Ok(traveler);
        }

        [HttpGet]
        [Authorize]
        [Route("managers")]
        [ProducesResponseType(typeof(ManagerViewModel), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ManagerViewModel>> GetManager()
        {
            try
            {
                var auth = (AuthViewModel?)HttpContext.Items["User"];
                if (auth != null)
                {
                    var manager = await _authService.GetManagerById(auth.Id);
                    return manager != null ? Ok(manager) : NotFound();
                }
                return BadRequest();
            }
            catch (Exception e)
            {
                return StatusCode(StatusCodes.Status400BadRequest, e.InnerException != null ? e.InnerException.Message : e.Message);
            }
        }

        [HttpGet]
        [Authorize]
        [Route("tour-guides")]
        [ProducesResponseType(typeof(TourGuideViewModel), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<TourGuideViewModel>> GetTourGuide()
        {
            try
            {
                var auth = (AuthViewModel?)HttpContext.Items["User"];
                if (auth != null)
                {
                    var tourGuide = await _authService.GetTourGuideById(auth.Id);
                    return tourGuide != null ? Ok(tourGuide) : NotFound();
                }
                return BadRequest();
            }
            catch (Exception e)
            {
                return StatusCode(StatusCodes.Status400BadRequest, e.InnerException != null ? e.InnerException.Message : e.Message);
            }
        }

        [HttpGet]
        [Authorize]
        [Route("travelers")]
        [ProducesResponseType(typeof(TravelerViewModel), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<TravelerViewModel>> GetTraveler()
        {
            try
            {
                var auth = (AuthViewModel?)HttpContext.Items["User"];
                if (auth != null)
                {
                    var traveler = await _authService.GetTravelerById(auth.Id);
                    return traveler != null ? Ok(traveler) : NotFound();
                }
                return BadRequest();
            }
            catch (Exception e)
            {
                return StatusCode(StatusCodes.Status400BadRequest, e.InnerException != null ? e.InnerException.Message : e.Message);
            }
        }
    }
}
