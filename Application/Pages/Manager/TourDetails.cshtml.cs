using Application.Commons;
using Data.EFCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Service.Interfaces;
using Service.Models.Schedule;
using Service.Models.Tour;
using Service.Models.Trip;
using Shared.ResultExtensions;

namespace Application.Pages.Manager;

public class TourDetails : PageModel
{
    private readonly ITourService _tourService;
    private readonly ITripService _tripService;
    private readonly UnitOfWork _unitOfWork;

    public TourDetails(ITourService tourService, ITripService tripService, UnitOfWork unitOfWork)
    {
        _tourService = tourService;
        _tripService = tripService;
        _unitOfWork = unitOfWork;
    }

    // Get
    public List<IGrouping<int, ScheduleViewModel>> ScheduleGroups = new();
    public TourDetailsViewModel Tour { get; set; }
    public List<TripViewModel> Trips { get; set; } = new();
    public UserSessionModel? CurrentUser { get; set; }

    // Post
    public IFormFile ImportFile { get; set; }
    public string? ErrorMessage { get; set; }

    public override void OnPageHandlerSelected(PageHandlerSelectedContext context)
    {
        CurrentUser = RazorPageHelper.GetUserFromSession(HttpContext.Session);
        base.OnPageHandlerSelected(context);
    }

    /// <summary>
    /// Delete Tour
    /// </summary>
    public async Task<IActionResult> OnGetDeleteTourAsync(Guid tourId)
    {
        // Auth
        if (CurrentUser is null) return RedirectToPage("Login");

        var tour = await _unitOfWork.Tours.Query()
            .Where(e => e.DeletedById == null)
            .FirstOrDefaultAsync(e => e.Id == tourId);

        if (tour != null)
        {
            tour.DeletedById = CurrentUser.Id;
            _unitOfWork.Tours.Update(tour);
            await _unitOfWork.SaveChangesAsync();
        }

        return RedirectToPage("Tours");
    }

    /// <summary>
    /// Import trip
    /// </summary>
    public async Task<IActionResult> OnPostAsync(Guid tourId)
    {
        // Auth
        if (CurrentUser is null) return RedirectToPage("Login");

        // Check if file exist
        if (ReferenceEquals(ImportFile, null))
        {
            ErrorMessage = "Choose a file";
        }
        // Import data
        else
        {
            var result = await _tripService.ImportTrip(
                CurrentUser.Id, tourId, ImportFile.OpenReadStream());

            // Error message
            ErrorMessage = result.IsSuccess ? "" : result.Error.ErrorDetails.FirstOrDefault();
        }

        await _loadPageData(tourId);
        return Page();
    }

    /// <summary>
    /// Get page
    /// </summary>
    public async Task<IActionResult> OnGetAsync(Guid tourId)
    {
        // Auth
        if (CurrentUser is null) return RedirectToPage("Login");

        //
        var loadResult = await _loadPageData(tourId);
        if (loadResult.IsSuccess) return Page();

        TempData["Error"] = loadResult.Error;
        return RedirectToPage("/Error");
    }

    private async Task<Result> _loadPageData(Guid tourId)
    {
        var tourResult = await _tourService.GetDetails(tourId);
        if (!tourResult.IsSuccess) return tourResult.Error;

        var tripsResult = await _tourService.ListTrips(tourId);
        if (!tripsResult.IsSuccess) return tripsResult.Error;

        Tour = tourResult.Value;
        Trips = tripsResult.Value;

        ScheduleGroups = Tour.Schedules
            .OrderBy(schedule => schedule.Sequence)
            .GroupBy(schedule => schedule.DayNo)
            .OrderBy(group => group.Key)
            .ToList();

        return Result.Success();
    }
}