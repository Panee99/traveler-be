﻿using Application.Configurations.Auth;
using Data.Enums;
using Microsoft.AspNetCore.Mvc;
using Service.Commons.QueryExtensions;
using Service.Interfaces;
using Service.Models.Schedule;
using Service.Models.Tour;
using Service.Models.Trip;

namespace Application.Controllers;

[Route("tours")]
public class ToursController : ApiController
{
    private readonly ITourService _tourService;
    private static readonly byte[] TourSampleData;
    private const string TourSampleFileName = "Tour-Sample.zip";

    static ToursController()
    {
        // Read sample data to memory
        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "Controllers", "Statics", TourSampleFileName);
        if (!System.IO.File.Exists(filePath)) throw new Exception($"'{TourSampleFileName}' file not found.");
        TourSampleData = System.IO.File.ReadAllBytes(filePath);
    }

    public ToursController(ITourService tourService)
    {
        _tourService = tourService;
    }

    /// <summary>
    /// Get tour excel sample file
    /// </summary>
    [HttpGet("import/sample")]
    public IActionResult DownloadFile()
    {
        return File(
            TourSampleData,
            "application/zip",
            TourSampleFileName
        );
    }

    /// <summary>
    /// Import tour excel file
    /// </summary>
    [Authorize(UserRole.Manager)]
    [HttpPost("import")]
    [ProducesResponseType(typeof(TourDetailsViewModel), StatusCodes.Status200OK)]
    public async Task<IActionResult> ImportTour(IFormFile file)
    {
        var result = await _tourService.ImportTour(CurrentUser.Id, file.OpenReadStream());
        return result.Match(Ok, OnError);
    }

    /// <summary>
    /// Delete a tour
    /// </summary>
    [Authorize(UserRole.Manager)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _tourService.Delete(id);
        return result.Match(Ok, OnError);
    }

    /// <summary>
    /// Get details of a tour
    /// </summary>
    [ProducesResponseType(typeof(TourDetailsViewModel), StatusCodes.Status200OK)]
    [HttpGet("{id:guid}/details")]
    public async Task<IActionResult> GetDetails(Guid id)
    {
        var result = await _tourService.GetDetails(id);
        return result.Match(Ok, OnError);
    }

    /// <summary>
    /// Filter tours
    /// </summary>
    [ProducesResponseType(typeof(PaginationModel<TourViewModel>), StatusCodes.Status200OK)]
    [HttpPost("filter")]
    public async Task<IActionResult> Filter(TourFilterModel model)
    {
        var result = await _tourService.Filter(model);
        return result.Match(Ok, OnError);
    }

    /// <summary>
    /// List all trips of a tour
    /// </summary>
    [ProducesResponseType(typeof(List<TripViewModel>), StatusCodes.Status200OK)]
    [HttpGet("{id:guid}/trips")]
    public async Task<IActionResult> ListTrips(Guid id)
    {
        var result = await _tourService.ListTrips(id);
        return result.Match(Ok, OnError);
    }

    /// <summary>
    /// Get schedules of a tour
    /// </summary>
    [ProducesResponseType(typeof(List<ScheduleViewModel>), StatusCodes.Status200OK)]
    [HttpGet("{id:guid}/schedules")]
    public async Task<IActionResult> ListSchedules(Guid id)
    {
        var result = await _tourService.ListSchedules(id);
        return result.Match(Ok, OnError);
    }
}