using Data.Models.Get;
using Data.Models.View;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service.Interfaces;
using System.ComponentModel.DataAnnotations;

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

        [Route("travellers")]
        [HttpPost]
        [ProducesResponseType(typeof(AuthViewModel), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<AuthViewModel>> AuthenticateTraveller([FromBody][Required] AuthRequestModel model)
        {
            var traveller = await _authService.AuthenticateTraveller(model);
            if (traveller is null)
            {
                return NotFound();
            }
            return Ok(traveller);
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
        [Route("travellers")]
        [ProducesResponseType(typeof(TravellerViewModel), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<TravellerViewModel>> GetTraveller()
        {
            try
            {
                var auth = (AuthViewModel?)HttpContext.Items["User"];
                if (auth != null)
                {
                    var traveller = await _authService.GetTravellerById(auth.Id);
                    return traveller != null ? Ok(traveller) : NotFound();
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
