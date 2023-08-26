using System.Globalization;
using Application.Commons;
using Data.EFCore;
using Data.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Service.Interfaces;
using Service.Models.Tour;

namespace Application.Pages.Manager;

[IgnoreAntiforgeryToken]
public class TripDetailsModel : PageModel
{
    private readonly UnitOfWork _unitOfWork;
    private readonly ITourService _tourService;
    public readonly ICloudStorageService CloudStorageService;

    public TripDetailsModel(
        UnitOfWork unitOfWork,
        ITourService tourService, ICloudStorageService cloudStorageService)
    {
        _unitOfWork = unitOfWork;
        _tourService = tourService;
        CloudStorageService = cloudStorageService;
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

        if (trip is null) return NotFound();

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
            .Where(trip => trip.DeletedById == null &&
                           trip.Tour.DeletedById == null)
            .Where(trip => trip.Id == tripId)
            .Include(trip => trip.TourGroups).ThenInclude(group => group.Travelers)
            .Include(trip => trip.TourGroups).ThenInclude(group => group.TourGuide)
            .Include(trip => trip.TourGroups).ThenInclude(group => group.IncurredCostActivities)
            .ThenInclude(cost => cost.Image)
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

    public static string FormatMoney(double amount)
    {
        var cultureInfo = new CultureInfo("vi-VN");
        cultureInfo.NumberFormat.CurrencySymbol = "₫";
        var formattedAmount = amount.ToString("C0", cultureInfo);

        return formattedAmount;
    }
}