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

        if (CurrentUser is not { Role: AccountRole.Manager })
            if (model.IdToken is null || !await _verifyIdToken(model.Phone, model.IdToken))
                return DomainErrors.Traveler.IdToken;

        var formattedPhone = _formatPhoneNum(model.Phone);

        if (await UnitOfWork.Accounts.AnyAsync(e => e.Phone == formattedPhone))
            return Error.Conflict("Account with this phone number already exist");

        UnitOfWork.Travelers.Add(
            new Traveler
            {
                Phone = formattedPhone,
                Password = AuthHelper.HashPassword(model.Password),
                Status = AccountStatus.Active,
                FirstName = model.FirstName,
                LastName = model.LastName,
                Gender = model.Gender,
                Role = AccountRole.Traveler
            }
        );

        await UnitOfWork.SaveChangesAsync();
        return Result.Success();
    }

    public async Task<Result<TravelerViewModel>> GetProfile(Guid id)
    {
        var entity = await UnitOfWork.Travelers
            .Query()
            .FirstOrDefaultAsync(e => e.Id == id && e.Status == AccountStatus.Active);

        if (entity is null) return Error.NotFound();
        return _mapper.Map<TravelerViewModel>(entity);
    }

    // TODO: Test
    public async Task<Result<List<TravelerViewModel>>> ListByTour(Guid tourId)
    {
        if (!await UnitOfWork.Tours.AnyAsync(e => e.Id == tourId))
            return Error.NotFound("Tour not found.");

        var travelers = await UnitOfWork.Tours.Query().Where(e => e.Id == tourId)
            .SelectMany(e => e.TourGroups)
            .SelectMany(e => e.Travelers)
            .DistinctBy(e => e.Id)
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
        var tours = await UnitOfWork.Travelers.Query().Where(e => e.Id == travelerId)
            .SelectMany(e => e.TourGroups)
            .Select(e => e.Tour).ToListAsync();

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