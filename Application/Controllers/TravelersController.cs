using Application.Configurations.Auth;
using Microsoft.AspNetCore.Mvc;
using Service.Interfaces;
using Service.Models.Traveler;
using Shared.Enums;
using Swashbuckle.AspNetCore.Annotations;

namespace Application.Controllers;

[Route("travelers")]
public class TravelersController : ApiController
{
    private readonly ITravelerService _travelerService;
    private readonly ILogger<TravelersController> _logger;

    public TravelersController(ITravelerService travelerService, ILogger<TravelersController> logger)
    {
        _travelerService = travelerService;
        _logger = logger;
    }

    [SwaggerOperation(Description = "Phone format: '84' or '+84'.")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [HttpPost("register")]
    public async Task<IActionResult> Register(TravelerRegistrationModel model)
    {
        if (!model.Phone.StartsWith('+')) model.Phone = '+' + model.Phone;
        var result = await _travelerService.Register(model);
        return result.Match(Ok, OnError);
    }

    [SwaggerOperation(Description = "Self profile")]
    [ProducesResponseType(typeof(TravelerProfileViewModel), StatusCodes.Status200OK)]
    [Authorize(UserRole.Traveler)]
    [HttpGet("profile")]
    public IActionResult GetSelfProfile()
    {
        var result = _travelerService.GetProfile(CurrentUser.Id);
        return result.Match(Ok, OnError);
    }
    
    [SwaggerOperation(Description = "All travelers profile")]
    [ProducesResponseType(typeof(TravelerProfileViewModel), StatusCodes.Status200OK)]
    [Authorize]
    [HttpGet("{id:guid}/profile")]
    public IActionResult GetProfile(Guid id)
    {
        var result = _travelerService.GetProfile(id);

        return result.Match(Ok, OnError);
    }
}