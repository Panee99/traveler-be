using Application.Configurations.Auth;
using Microsoft.AspNetCore.Mvc;
using Service.Interfaces;
using Service.Models.Traveler;
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

    [SwaggerOperation(Description = "Leave 'id' empty to get self profile")]
    [ProducesResponseType(typeof(TravelerProfileViewModel), StatusCodes.Status200OK)]
    [Authorize]
    [HttpGet("profile")]
    public IActionResult GetProfile(Guid? id)
    {
        id ??= CurrentUser.Id;

        var result = _travelerService.GetProfile(id.Value);

        return result.Match(Ok, OnError);
    }
}