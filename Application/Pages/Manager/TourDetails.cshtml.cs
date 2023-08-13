using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Service.Interfaces;
using Service.Models.Schedule;
using Service.Models.Tour;
using Service.Models.Trip;

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

    // Post
    public IFormFile ImportFile { get; set; }

    public async Task<IActionResult> OnPostAsync()
    {
        var result = await _tripService.ImportTrip(
            Guid.Parse("69f7719f-be55-42ca-843e-bc46cd1b450d"),
            ImportFile.OpenReadStream());

        if (!result.IsSuccess)
        {
            return RedirectToPage("TourDetails");
        }

        return RedirectToPage("TourDetails");
    }

    public async Task OnGetAsync(Guid id)
    {
        var tourResult = await _tourService.GetDetails(id);
        if (tourResult.IsSuccess) Tour = tourResult.Value;

        var tripsResult = await _tourService.ListTrips(id);
        if (tripsResult.IsSuccess) Trips = tripsResult.Value;

        ScheduleGroups = Tour.Schedules
            .OrderBy(schedule => schedule.Sequence)
            .GroupBy(schedule => schedule.DayNo)
            .OrderBy(group => group.Key)
            .ToList();
    }
}