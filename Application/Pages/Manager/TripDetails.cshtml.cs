using Data.EFCore;
using Data.Entities;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Service.Interfaces;
using Service.Models.Tour;

namespace Application.Pages.Manager;

public class TripDetails : PageModel
{
    private readonly UnitOfWork _unitOfWork;
    private readonly ICloudStorageService _cloudStorageService;
    private readonly ITourService _tourService;

    public TripDetails(
        UnitOfWork unitOfWork,
        ICloudStorageService cloudStorageService,
        ITourService tourService)
    {
        _unitOfWork = unitOfWork;
        _cloudStorageService = cloudStorageService;
        _tourService = tourService;
    }

    public Trip Trip { get; set; }
    public TourDetailsViewModel Tour { get; set; }

    public async Task OnGetAsync(Guid id)
    {
        var trip = await _unitOfWork.Trips
            .Query()
            .Where(trip => trip.Id == id)
            .Include(trip => trip.TourGroups).ThenInclude(group => group.Travelers)
            .Include(trip => trip.TourGroups).ThenInclude(group => group.TourGuide)
            .FirstOrDefaultAsync();

        if (trip != null) Trip = trip;

        var tourResult = await _tourService.GetDetails(Trip.TourId);
        if (tourResult.IsSuccess) Tour = tourResult.Value;
    }
}