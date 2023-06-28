using Application.Configurations.Auth;
using Microsoft.AspNetCore.Mvc;
using Service.Interfaces;
using Service.Models.TourGroup;
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

    /// <summary>
    /// Register for new account (mobile)
    /// </summary>
    [SwaggerOperation(Description = "Phone format: '84' or '+84'.")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [HttpPost("register")]
    public async Task<IActionResult> Register(TravelerRegistrationModel model)
    {
        var result = await _travelerService.Register(model);
        return result.Match(Ok, OnError);
    }

    /// <summary>
    /// List all bookings of a traveler
    /// </summary>
    [Authorize]
    [HttpGet("{id:guid}/booked")]
    public async Task<IActionResult> ListBooked(Guid id)
    {
        var result = await _bookingService.ListTravelerBooked(id);
        return result.Match(Ok, OnError);
    }

    [ProducesResponseType(typeof(TourGroupViewModel), StatusCodes.Status200OK)]
    [Authorize]
    [HttpGet("{id:guid}/joined-groups")]
    public async Task<IActionResult> ListJoinedGroups(Guid id)
    {
        var result = await _travelerService.ListJoinedGroups(id);
        return result.Match(Ok, OnError);
    }

    [ProducesResponseType(typeof(TourGroupViewModel), StatusCodes.Status200OK)]
    [Authorize]
    [HttpGet("{id:guid}/current-group")]
    public async Task<IActionResult> GetCurrentJoinedGroup(Guid id)
    {
        var result = await _travelerService.GetCurrentJoinedGroup(id);
        return result.Match(Ok, OnError);
    }

    [ProducesResponseType(typeof(TravelerTravelInfo), StatusCodes.Status200OK)]
    [Authorize]
    [HttpGet("{id:guid}/travel-info")]
    public async Task<IActionResult> GetTravelerTravelInfo(Guid id)
    {
        var result = await _travelerService.GetTravelerTravelInfo(id);
        return result.Match(Ok, OnError);
    }
}