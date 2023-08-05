using Microsoft.AspNetCore.Mvc.RazorPages;
using Service.Interfaces;
using Service.Models.Schedule;
using Service.Models.Tour;
using Service.Models.Trip;

namespace Application.Pages.Manager;

public class TourDetails : PageModel
{
    private readonly ITourService _tourService;

    public TourDetails(ITourService tourService)
    {
        _tourService = tourService;
    }

    public List<IGrouping<int, ScheduleViewModel>> ScheduleGroups = new();
    public TourDetailsViewModel Tour { get; set; }
    public List<TripViewModel> Trips { get; set; } = new();

    public async Task OnGet(Guid id)
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