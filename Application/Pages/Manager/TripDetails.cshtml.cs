using Application.Commons;
using Data.EFCore;
using Data.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Service;
using Service.Interfaces;
using Service.Models.Tour;
using Shared.ResultExtensions;

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
    public UserSessionModel? CurrentUser { get; set; }

    public override void OnPageHandlerSelected(PageHandlerSelectedContext context)
    {
        CurrentUser = RazorPageHelper.GetUserFromSession(HttpContext.Session);
        base.OnPageHandlerSelected(context);
    }

    /// <summary>
    /// Delete Trip
    /// </summary>
    public async Task<IActionResult> OnGetDeleteTripAsync(Guid tripId)
    {
        // Auth
        if (CurrentUser is null) return RedirectToPage("Login");

        var trip = await _unitOfWork.Trips.Query()
            .Where(e => e.DeletedById == null)
            .FirstOrDefaultAsync(e => e.Id == tripId);

        if (trip is null) return RedirectToPage("Error");

        trip.DeletedById = CurrentUser.Id;
        _unitOfWork.Trips.Update(trip);
        await _unitOfWork.SaveChangesAsync();

        return RedirectToPage("TourDetails", new { trip.TourId });
    }

    /// <summary>
    /// Get page
    /// </summary>
    public async Task<IActionResult> OnGetAsync(Guid tripId)
    {
        // Auth
        if (CurrentUser is null) return RedirectToPage("Login");

        // Get Trip
        var trip = await _unitOfWork.Trips.Query()
            .Where(trip => trip.DeletedById == null)
            .Where(trip => trip.Id == tripId)
            .Include(trip => trip.TourGroups).ThenInclude(group => group.Travelers)
            .Include(trip => trip.TourGroups).ThenInclude(group => group.TourGuide)
            .AsSplitQuery()
            .FirstOrDefaultAsync();

        // Return 404 if trip not found
        if (trip is null) return NotFound();

        Trip = trip;
        var tourResult = await _tourService.GetDetails(Trip.TourId);
        if (tourResult.IsSuccess) Tour = tourResult.Value;
        return Page();
    }

    /// <summary>
    /// Combine Traveler and TourGuide into list of members 
    /// </summary>
    public List<User> GetUsersInGroup(TourGroup group)
    {
        var users = group.Travelers.Select(t => (User)t).ToList();
        if (group.TourGuide != null) users.Insert(0, group.TourGuide);
        return users;
    }
}