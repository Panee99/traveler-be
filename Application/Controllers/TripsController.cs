using Application.Configurations.Auth;
using Data.Enums;
using Microsoft.AspNetCore.Mvc;
using Service.Interfaces;
using Service.Models.TourGroup;
using Service.Models.Trip;
using Service.Models.Weather;

namespace Application.Controllers;

[Route("trips")]
public class TripsController : ApiController
{
    private readonly ITripService _tripService;
    private static readonly byte[] TripSampleData;
    private const string TripSampleFileName = "Trip-Sample.zip";

    static TripsController()
    {
        // Read sample data to memory
        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "Controllers", "Statics", TripSampleFileName);
        if (!System.IO.File.Exists(filePath)) throw new Exception($"'{TripSampleFileName}' file not found.");
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
            TripSampleFileName
        );
    }

    /// <summary>
    /// Import trip excel file
    /// </summary>
    [Authorize(UserRole.Manager)]
    [HttpPost("import")]
    public async Task<IActionResult> TripImport(Guid tourId, IFormFile file)
    {
        var result = await _tripService.ImportTrip(CurrentUser.Id, tourId, file.OpenReadStream());
        return result.Match(Ok, OnError);
    }

    /// <summary>
    /// Delete a trip
    /// </summary>
    [Authorize(UserRole.Manager)]
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
    [Authorize]
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
    [Authorize]
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
    [Authorize]
    [ProducesResponseType(typeof(List<WeatherAlertViewModel>), StatusCodes.Status200OK)]
    [HttpGet("{id:guid}/weather-alerts")]
    public async Task<IActionResult> ListWeatherAlerts(Guid id)
    {
        var result = await _tripService.ListWeatherAlerts(id);
        return result.Match(Ok, OnError);
    }

    // /// <summary>
    // /// List weather forecasts of a trip
    // /// </summary>
    // [Authorize]
    // [ProducesResponseType(typeof(List<WeatherForecastViewModel>), StatusCodes.Status200OK)]
    // [HttpGet("{id:guid}/weather-forecasts")]
    // public async Task<IActionResult> ListWeatherForecasts(Guid id)
    // {
    //     var result = await _tripService.ListWeatherForecasts(id);
    //     return result.Match(Ok, OnError);
    // }
}