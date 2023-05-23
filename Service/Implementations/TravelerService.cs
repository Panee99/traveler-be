using Data.EFCore;
using Data.Entities;
using Data.Enums;
using FirebaseAdmin.Auth;
using Mapster;
using MapsterMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Service.Errors;
using Service.Interfaces;
using Service.Models.Tour;
using Service.Models.Traveler;
using Shared.Helpers;
using Shared.ResultExtensions;

namespace Service.Implementations;

public class TravelerService : BaseService, ITravelerService
{
    private readonly ILogger<TravelerService> _logger;
    private readonly IMapper _mapper;
    private readonly ICloudStorageService _cloudStorageService;

    public TravelerService(UnitOfWork unitOfWork, IMapper mapper,
        ILogger<TravelerService> logger, IHttpContextAccessor httpContextAccessor,
        ICloudStorageService cloudStorageService)
        : base(unitOfWork, httpContextAccessor)
    {
        _mapper = mapper;
        _logger = logger;
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

    // TODO: Test
    public async Task<Result<List<TourFilterViewModel>>> ListJoinedTours(Guid travelerId)
    {
        var tours = await UnitOfWork.Travelers.Query()
            .Where(traveler => traveler.Id == travelerId)
            .SelectMany(traveler => traveler.TourGroups)
            .Select(group => group.TourVariant)
            .Select(variant => variant.Tour).ToListAsync();

        var views = tours.Select(tour =>
        {
            var view = tour.Adapt<TourFilterViewModel>();
            if (tour.ThumbnailId != null)
                view.ThumbnailUrl = _cloudStorageService.GetMediaLink(tour.ThumbnailId.Value);
            return view;
        }).ToList();

        return views;
    }

    // PRIVATE

    #region PRIVATE

    private string _formatPhoneNum(string phone)
    {
        if (phone.StartsWith('+')) phone = phone.Substring(1);
        return phone;
    }

    private async Task<bool> _verifyIdToken(string phone, string idToken)
    {
        try
        {
            var firebaseToken = await FirebaseAuth.DefaultInstance.VerifyIdTokenAsync(idToken);
            var tokenPhone = firebaseToken.Claims["phone_number"];
            if (phone.Equals(tokenPhone)) return true;
        }
        catch (Exception e)
        {
            _logger.LogDebug(e, "{Message}", typeof(TravelerService).ToString());
        }

        return false;
    }

    #endregion
}