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

    public TravelerService(UnitOfWork unitOfWork,
        IHttpContextAccessor httpContextAccessor,
        ICloudStorageService cloudStorageService)
        : base(unitOfWork, httpContextAccessor)
    {
        _cloudStorageService = cloudStorageService;
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

    // TODO: Test
    public async Task<Result<List<TravelerViewModel>>> ListByTourVariant(Guid tourVariantId)
    {
        if (!await UnitOfWork.TourVariants.AnyAsync(e => e.Id == tourVariantId))
            return Error.NotFound("Tour not found.");

        var travelers = await UnitOfWork.TourVariants.Query()
            .Where(variant => variant.Id == tourVariantId)
            .SelectMany(variant => variant.TourGroups)
            .SelectMany(group => group.Travelers)
            .ToListAsync();

        var views = travelers.Select(e =>
        {
            var view = e.Adapt<TravelerViewModel>();
            if (e.AvatarId != null) view.AvatarUrl = _cloudStorageService.GetMediaLink(e.AvatarId.Value);
            return view;
        }).ToList();

        return views;
    }

    public async Task<Result<List<TourGroupViewModel>>> ListJoinedGroups(Guid travelerId)
    {
        var groups = await UnitOfWork.Travelers
            .Query()
            .Where(traveler => traveler.Id == travelerId)
            .SelectMany(traveler => traveler.TourGroups)
            .Include(group => group.TourVariant)
            .ThenInclude(variant => variant.Tour)
            .ToListAsync();

        var views = groups.Select(group =>
        {
            var tour = group.TourVariant.Tour;
            var view = group.Adapt<TourGroupViewModel>();
            view.TourVariant!.Tour!.ThumbnailUrl = tour.ThumbnailId is null
                ? null
                : _cloudStorageService.GetMediaLink(tour.ThumbnailId.Value);

            return view;
        }).ToList();

        return views;
    }

    public async Task<Result<TourGroupViewModel>> GetCurrentJoinedGroup(Guid travelerId)
    {
        // Check traveler exist
        if (!await UnitOfWork.Travelers.AnyAsync(e => e.Id == travelerId))
            return Error.NotFound("Traveler not found.");

        // Get traveler current joined group
        var currentGroup = await UnitOfWork.Travelers
            .Query()
            .Where(traveler => traveler.Id == travelerId)
            .SelectMany(guide => guide.TourGroups)
            .Include(group => group.TourVariant)
            .ThenInclude(variant => variant.Tour)
            .Where(group => group.TourVariant.Status != TourVariantStatus.Ended &&
                            group.TourVariant.Status != TourVariantStatus.Canceled)
            .FirstOrDefaultAsync();

        if (currentGroup is null) return Error.NotFound("No current tour group joined");

        // Map to view model
        var tour = currentGroup.TourVariant.Tour;
        var view = currentGroup.Adapt<TourGroupViewModel>();
        
        view.TourVariant!.Tour!.ThumbnailUrl = tour.ThumbnailId is null
            ? null
            : _cloudStorageService.GetMediaLink(tour.ThumbnailId!.Value);

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