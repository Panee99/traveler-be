using Data.EFCore;
using Data.Entities;
using Data.Enums;
using Mapster;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Service.Interfaces;
using Service.Models.TourGroup;
using Service.Models.Traveler;
using Service.Models.User;
using Shared.Helpers;
using Shared.ResultExtensions;

namespace Service.Implementations;

public class TravelerService : BaseService, ITravelerService
{
    private readonly ICloudStorageService _cloudStorageService;

    public TravelerService(UnitOfWork unitOfWork,
        IHttpContextAccessor httpContextAccessor,
        ICloudStorageService cloudStorageService) : base(unitOfWork, httpContextAccessor)
    {
        _cloudStorageService = cloudStorageService;
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