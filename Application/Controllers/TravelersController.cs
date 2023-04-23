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
    private readonly IBookingService _bookingService;
    
    public TravelersController(ITravelerService travelerService, IBookingService bookingService)
    {
        _travelerService = travelerService;
        _bookingService = bookingService;
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

    [ProducesResponseType(typeof(TravelerProfileViewModel), StatusCodes.Status200OK)]
    [Authorize]
    [HttpGet("{id:guid}/profile")]
    public async Task<IActionResult> GetProfile(Guid id)
    {
        var result = await _travelerService.GetProfile(id);
        return result.Match(Ok, OnError);
    }

    [Authorize]
    [HttpGet("/tours/{tourId:guid}/travelers")]
    public async Task<IActionResult> ListByTour(Guid tourId)
    {
        var result = await _travelerService.ListByTour(tourId);
        return result.Match(Ok, OnError);
    }

    [Authorize]
    [HttpGet("{travelerId:guid}/booked")]
    public async Task<IActionResult> ListBooked(Guid travelerId)
    {
        var result = await _bookingService.ListTravelerBooked(travelerId);
        return result.Match(Ok, OnError);
    }
}