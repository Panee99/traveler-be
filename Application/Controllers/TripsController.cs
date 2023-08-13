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

    private static readonly byte[] TripSampleData;

    static TripsController()
    {
        // Read sample data to memory
        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "Controllers", "Statics", "Trip.zip");
        if (!System.IO.File.Exists(filePath)) throw new Exception("'Trip.zip' file not found.");
        TripSampleData = System.IO.File.ReadAllBytes(filePath);
    }

    public TripsController(ITripService tripService)
    {
        _tripService = tripService;
    }

    /// <summary>
    /// Get trip excel sample file
    /// </summary>
    [AllowAnonymous]
    [HttpGet("import/sample")]
    public IActionResult DownloadFile()
    {
        return File(
            TripSampleData,
            "application/zip",
            "Trip.zip"
        );
    }

    /// <summary>
    /// Import trip excel file
    /// </summary>
    [HttpPost("import")]
    public async Task<IActionResult> TripImport(IFormFile file)
    {
        var result = await _tripService.ImportTrip(CurrentUser.Id, file.OpenReadStream());
        return result.Match(Ok, OnError);
    }

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
    /// List weather alerts of a trip
    /// </summary>
    [ProducesResponseType(typeof(List<WeatherAlertViewModel>), StatusCodes.Status200OK)]
    [HttpGet("{id:guid}/weather-alerts")]
    public async Task<IActionResult> ListWeatherAlerts(Guid id)
    {
        var result = await _tripService.ListWeatherAlerts(id);
        return result.Match(Ok, OnError);
    }

    /// <summary>
    /// List weather forecasts of a trip
    /// </summary>
    [ProducesResponseType(typeof(List<WeatherForecastViewModel>), StatusCodes.Status200OK)]
    [HttpGet("{id:guid}/weather-forecasts")]
    public async Task<IActionResult> ListWeatherForecasts(Guid id)
    {
        var result = await _tripService.ListWeatherForecasts(id);
        return result.Match(Ok, OnError);
    }
}