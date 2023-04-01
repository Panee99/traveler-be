using Application.Commons;
using Application.Configurations.Auth;
using Microsoft.AspNetCore.Mvc;
using Service.Interfaces;
using Service.Models.Traveler;
using Swashbuckle.AspNetCore.Annotations;

namespace Application.Controllers;

[Route("travelers")]
public class TravelersController : ApiController
{
    private readonly ILogger<TravelersController> _logger;
    private readonly ITravelerService _travelerService;

    public TravelersController(ITravelerService travelerService, ILogger<TravelersController> logger)
    {
        _travelerService = travelerService;
        _logger = logger;
    }

    [SwaggerOperation(
        Summary = "No need idToken for manager",
        Description = "Phone format: '84' or '+84'.")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [HttpPost("register")]
    public async Task<IActionResult> Register(TravelerRegistrationModel model)
    {
        var result = await _travelerService.Register(model);
        return result.Match(Ok, OnError);
    }

    [SwaggerOperation(Description = "All travelers profile")]
    [ProducesResponseType(typeof(TravelerProfileViewModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponsePayload), StatusCodes.Status404NotFound)]
    [Authorize]
    [HttpGet("{id:guid}/profile")]
    public async Task<IActionResult> GetProfile(Guid id)
    {
        var result = await _travelerService.GetProfile(id);
        return result.Match(Ok, OnError);
    }
}