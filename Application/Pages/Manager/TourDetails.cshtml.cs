using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
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

    public TourDetails(ITourService tourService, ITripService tripService)
    {
        _tourService = tourService;
        _tripService = tripService;
    }

    // Get
    public List<IGrouping<int, ScheduleViewModel>> ScheduleGroups = new();
    public TourDetailsViewModel Tour { get; set; }
    public List<TripViewModel> Trips { get; set; } = new();
    public UserSessionModel CurrentUser { get; set; }
    
    // Post
    public IFormFile ImportFile { get; set; }
    public string? ErrorMessage { get; set; }

    public async Task<IActionResult> OnPostAsync(Guid tourId)
    {
        if (ReferenceEquals(ImportFile, null))
        {
            ErrorMessage = "Choose a file";
        }
        else
        {
            var result = await _tripService.ImportTrip(
                Guid.Parse("69f7719f-be55-42ca-843e-bc46cd1b450d"),
                ImportFile.OpenReadStream());

            ErrorMessage = result.IsSuccess ? "" : result.Error.ErrorDetails.FirstOrDefault();
        }

        await _loadPageData(tourId);
        return Page();
    }

    public async Task<IActionResult> OnGetAsync(Guid tourId)
    {
        // Authenticate User
        var userSessionData = HttpContext.Session.GetString("User");
        var userData = userSessionData is null
            ? null
            : JsonConvert.DeserializeObject<UserSessionModel>(userSessionData);
        if (userData is null) return RedirectToPage("Login");
        CurrentUser = userData;
        
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