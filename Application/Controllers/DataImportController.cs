using Microsoft.AspNetCore.Mvc;
using Service.Interfaces;
using Service.Models.Tour;

namespace Application.Controllers;

[Route("import")]
public class DataImportController : ApiController
{
    private static readonly byte[] TourSampleData;

    static DataImportController()
    {
        // Read tour sample data to memory
        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "Statics", "Tour.xlsx");
        if (!System.IO.File.Exists(filePath)) throw new Exception("'Tour.xlsx' file not found.");
        TourSampleData = System.IO.File.ReadAllBytes(filePath);
    }

    private readonly IDataImportService _dataImportService;

    public DataImportController(IDataImportService dataImportService)
    {
        _dataImportService = dataImportService;
    }

    /// <summary>
    /// Get tour excel sample file
    /// </summary>
    [HttpGet("tour-sample")]
    public IActionResult DownloadFile()
    {
        return File(
            TourSampleData,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            "Tour.xlsx"
        );
    }

    /// <summary>
    /// Import tour excel file
    /// </summary>
    [HttpPost("tours")]
    [ProducesResponseType(typeof(TourDetailsViewModel), StatusCodes.Status200OK)]
    public async Task<IActionResult> TourImport(IFormFile file)
    {
        var result = await _dataImportService.ImportTour(file.OpenReadStream());
        return result.Match(Ok, OnError);
    }

    /// <summary>
    /// Import trip excel file
    /// </summary>
    [HttpPost("trips")]
    public async Task<IActionResult> TripImport(IFormFile file)
    {
        var result = await _dataImportService.ImportTrip(file.OpenReadStream());
        return result.Match(Ok, OnError);
    }
}