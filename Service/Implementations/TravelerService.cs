using Data.EFCore;
using Data.Entities;
using Data.Enums;
using Mapster;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Service.Interfaces;
using Service.Models.TourGroup;
using Service.Models.Traveler;
using Shared.Helpers;
using Shared.ResultExtensions;

namespace Service.Implementations;

public class TravelerService : BaseService, ITravelerService
{
    private readonly ICloudStorageService _cloudStorageService;
    private readonly ITourGroupService _tourGroupService;

    public TravelerService(UnitOfWork unitOfWork,
        IHttpContextAccessor httpContextAccessor,
        ICloudStorageService cloudStorageService,
        ITourGroupService tourGroupService)
        : base(unitOfWork, httpContextAccessor)
    {
        _cloudStorageService = cloudStorageService;
        _tourGroupService = tourGroupService;
    }

    public async Task<Result> Register(TravelerRegistrationModel model)
    {
        if (!model.Phone.StartsWith('+')) model.Phone = '+' + model.Phone;

        if (!model.Phone.StartsWith("+84")) return Error.Validation();

        var formattedPhone = _formatPhoneNum(model.Phone);

        if (await UnitOfWork.Users.AnyAsync(e => e.Phone == formattedPhone))
            return Error.Conflict("UserUser with this phone number already exist");

        UnitOfWork.Travelers.Add(
            new Traveler
            {
                Phone = formattedPhone,
                Password = AuthHelper.HashPassword(model.Password),
                Status = UserStatus.Active,
                FirstName = model.FirstName,
                LastName = model.LastName,
                Gender = model.Gender,
                Role = UserRole.Traveler
            }
        );

        await UnitOfWork.SaveChangesAsync();
        return Result.Success();
    }

    public async Task<Result<List<TourGroupViewModel>>> ListJoinedGroups(Guid travelerId)
    {
        var groupResults = await UnitOfWork.Travelers
            .Query()
            .AsSplitQuery()
            .Where(traveler => traveler.Id == travelerId)
            .SelectMany(traveler => traveler.TourGroups)
            .Include(group => group.Trip)
            .ThenInclude(trip => trip.Tour)
            .Select(group => new { Group = group, TravelerCount = group.Travelers.Count })
            .ToListAsync();

        // return
        return groupResults.Select(groupResult =>
        {
            var view = groupResult.Group.Adapt<TourGroupViewModel>();
            view.Trip!.Tour!.ThumbnailUrl =
                _cloudStorageService.GetMediaLink(groupResult.Group.Trip.Tour.ThumbnailId);
            view.TravelerCount = groupResult.TravelerCount;
            return view;
        }).ToList();
    }

    public async Task<Result<TourGroupViewModel>> GetCurrentJoinedGroup(Guid travelerId)
    {
        // Check traveler exist
        if (!await UnitOfWork.Travelers.AnyAsync(e => e.Id == travelerId))
            return Error.NotFound("Traveler not found.");

        // Get traveler current joined group
        var currentGroup = await UnitOfWork.Travelers
            .Query()
            .AsSplitQuery()
            .Where(traveler => traveler.Id == travelerId)
            .SelectMany(guide => guide.TourGroups)
            .Include(group => group.Trip)
            .ThenInclude(trip => trip.Tour)
            .Where(group => group.Trip.Status != TripStatus.Ended &&
                            group.Trip.Status != TripStatus.Canceled)
            .FirstOrDefaultAsync();

        if (currentGroup is null) return Error.NotFound("No current tour group joined");

        // Map to view model
        var tour = currentGroup.Trip.Tour;
        var view = currentGroup.Adapt<TourGroupViewModel>();
        view.Trip!.Tour!.ThumbnailUrl = _cloudStorageService.GetMediaLink(tour.ThumbnailId);
        view.TravelerCount = await _tourGroupService.CountTravelers(view.Id);

        return view;
    }

    // PRIVATE

    #region PRIVATE

    private string _formatPhoneNum(string phone)
    {
        if (phone.StartsWith('+')) phone = phone.Substring(1);
        return phone;
    }

    // private async Task<bool> _verifyIdToken(string phone, string idToken)
    // {
    //     try
    //     {
    //         var firebaseToken = await FirebaseAuth.DefaultInstance.VerifyIdTokenAsync(idToken);
    //         var tokenPhone = firebaseToken.Claims["phone_number"];
    //         if (phone.Equals(tokenPhone)) return true;
    //     }
    //     catch (Exception e)
    //     {
    //         _logger.LogDebug(e, "{Message}", typeof(TravelerService).ToString());
    //     }
    //
    //     return false;
    // }

    #endregion
}