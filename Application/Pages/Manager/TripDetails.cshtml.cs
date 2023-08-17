using Data.EFCore;
using Data.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Service.Interfaces;
using Service.Models.Tour;

namespace Application.Pages.Manager;

public class TripDetails : PageModel
{
    private readonly UnitOfWork _unitOfWork;
    private readonly ITourService _tourService;
    
    public TripDetails(
        UnitOfWork unitOfWork,
        ITourService tourService)
    {
        _unitOfWork = unitOfWork;
        _tourService = tourService;
    }

    public Trip Trip { get; set; }
    public TourDetailsViewModel Tour { get; set; }
    public UserSessionModel CurrentUser { get; set; }

    
    public List<User> GetUsersInGroup(TourGroup group)
    {
        var users = group.Travelers.Select(t => (User)t).ToList();
        if (group.TourGuide != null) users.Insert(0, group.TourGuide);
        return users;
    }

    public async Task<IActionResult> OnGetAsync(Guid tripId)
    {
        // Authenticate User
        var userSessionData = HttpContext.Session.GetString("User");
        var userData = userSessionData is null
            ? null
            : JsonConvert.DeserializeObject<UserSessionModel>(userSessionData);
        if (userData is null) return RedirectToPage("Login");
        CurrentUser = userData;

        //
        var trip = await _unitOfWork.Trips
            .Query()
            .Where(trip => trip.Id == tripId)
            .Include(trip => trip.TourGroups).ThenInclude(group => group.Travelers)
            .Include(trip => trip.TourGroups).ThenInclude(group => group.TourGuide)
            .AsSplitQuery()
            .FirstOrDefaultAsync();

        if (trip is null)
        {
            return NotFound("Trip not found");
        }

        Trip = trip;
        var tourResult = await _tourService.GetDetails(Trip.TourId);
        if (tourResult.IsSuccess) Tour = tourResult.Value;

        return Page();
    }
}