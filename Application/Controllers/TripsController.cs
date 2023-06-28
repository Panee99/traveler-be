using Application.Configurations.Auth;
using Microsoft.AspNetCore.Mvc;
using Service.Interfaces;
using Service.Models.Trip;

namespace Application.Controllers;

[Authorize]
[Route("trips")]
public class TripsController : ApiController
{
    private readonly ITravelerService _travelerService;
    private readonly ITripService _tripService;

    public TripsController(ITravelerService travelerService, ITripService tripService)
    {
        _travelerService = travelerService;
        _tripService = tripService;
    }

    /// <summary>
    /// Create a trip
    /// </summary>
    [HttpPost("")]
    public async Task<IActionResult> Create(TripCreateModel model)
    {
        var result = await _tripService.Create(model);
        return result.Match(Ok, OnError);
    }

    /// <summary>
    /// Update a trip
    /// </summary>
    [HttpPatch("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, TripUpdateModel model)
    {
        var result = await _tripService.Update(id, model);
        return result.Match(Ok, OnError);
    }

    /// <summary>
    /// Delete a trip
    /// </summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _tripService.Delete(id);
        return result.Match(Ok, OnError);
    }

    /// <summary>
    /// Get a trip
    /// </summary>
    [AllowAnonymous]
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id)
    {
        var result = await _tripService.Get(id);
        return result.Match(Ok, OnError);
    }

    /// <summary>
    /// List groups of a trip
    /// </summary>
    [HttpGet("{id:guid}/tour-groups")]
    public async Task<IActionResult> ListGroupsInTrip(Guid id)
    {
        var result = await _tripService.ListGroupsInTrip(id);
        return result.Match(Ok, OnError);
    }
}