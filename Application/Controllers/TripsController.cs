using Application.Configurations.Auth;
using Microsoft.AspNetCore.Mvc;
using Service.Interfaces;
using Service.Models.TourGroup;
using Service.Models.Trip;
using Service.Models.Weather;

namespace Application.Controllers;

[Authorize]
[Route("trips")]
public class TripsController : ApiController
{
    private readonly ITripService _tripService;

    public TripsController(ITripService tripService)
    {
        _tripService = tripService;
    }

    // /// <summary>
    // /// Create a trip
    // /// </summary>
    // [ProducesResponseType(typeof(TripViewModel), StatusCodes.Status200OK)]
    // [HttpPost("")]
    // public async Task<IActionResult> Create(TripCreateModel model)
    // {
    //     var result = await _tripService.Create(model);
    //     return result.Match(Ok, OnError);
    // }

    /// <summary>
    /// Update a trip
    /// </summary>
    [ProducesResponseType(typeof(TripViewModel), StatusCodes.Status200OK)]
    [HttpPatch("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, TripUpdateModel model)
    {
        var result = await _tripService.Update(id, model);
        return result.Match(Ok, OnError);
    }

    /// <summary>
    /// Delete a trip
    /// </summary>
    [ProducesResponseType(StatusCodes.Status200OK)]
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
    [ProducesResponseType(typeof(TripViewModel), StatusCodes.Status200OK)]
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id)
    {
        var result = await _tripService.Get(id);
        return result.Match(Ok, OnError);
    }

    /// <summary>
    /// List groups of a trip
    /// </summary>
    [ProducesResponseType(typeof(List<TourGroupViewModel>), StatusCodes.Status200OK)]
    [HttpGet("{id:guid}/tour-groups")]
    public async Task<IActionResult> ListGroupsInTrip(Guid id)
    {
        var result = await _tripService.ListGroupsInTrip(id);
        return result.Match(Ok, OnError);
    }

    /// <summary>
    /// List weather forecasts and alerts of a trip
    /// </summary>
    [ProducesResponseType(typeof(WeatherViewModel), StatusCodes.Status200OK)]
    [HttpGet("{id:guid}/weather")]
    public async Task<IActionResult> GetWeather(Guid id)
    {
        var result = await _tripService.GetWeather(id);
        return result.Match(Ok, OnError);
    }
}