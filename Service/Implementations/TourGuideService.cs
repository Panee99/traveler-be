using Data.EFCore;
using Data.Entities;
using Data.Enums;
using Mapster;
using Microsoft.EntityFrameworkCore;
using Service.Interfaces;
using Service.Models.Tour;
using Service.Models.TourGuide;
using Shared.Helpers;
using Shared.ResultExtensions;
using TourGuideViewModel = Service.Models.TourGuide.TourGuideViewModel;

namespace Service.Implementations;

public class TourGuideService : BaseService, ITourGuideService
{
    private readonly ICloudStorageService _cloudStorageService;

    public TourGuideService(UnitOfWork unitOfWork, ICloudStorageService cloudStorageService) : base(unitOfWork)
    {
        _cloudStorageService = cloudStorageService;
    }

    public async Task<Result<TourGuideViewModel>> Create(TourGuideCreateModel model)
    {
        if (await UnitOfWork.TourGuides.AnyAsync(e => e.Phone == model.Phone))
            return Error.Conflict("Phone number already exist");

        if (await UnitOfWork.TourGuides.AnyAsync(e => e.Email == model.Email))
            return Error.Conflict("Email already exist");

        var tourGuide = new TourGuide()
        {
            Phone = model.Phone,
            Email = model.Email,
            Password = AuthHelper.HashPassword(model.Password),
            FirstName = model.FirstName,
            LastName = model.LastName,
            Gender = model.Gender,
            Birthday = model.Birthday,
            Role = UserRole.TourGuide,
            Status = UserStatus.Active,
        };

        UnitOfWork.TourGuides.Add(tourGuide);

        await UnitOfWork.SaveChangesAsync();

        return tourGuide.Adapt<TourGuideViewModel>();
    }

    public async Task<Result<List<TourFilterViewModel>>> ListAssignedTours(Guid tourGuideId)
    {
        if (!await UnitOfWork.TourGuides.AnyAsync(e => e.Id == tourGuideId))
            return Error.NotFound("Tour Guide not found.");

        var assignedTours = await UnitOfWork.TourGuides.Query()
            .SelectMany(guide => guide.TourGroups)
            .Select(group => group.Tour)
            .ToListAsync();

        var views = assignedTours.Select(e =>
        {
            var view = e.Adapt<TourFilterViewModel>();
            if (e.ThumbnailId != null)
                view.ThumbnailUrl = _cloudStorageService.GetMediaLink(e.ThumbnailId.Value);

            return view;
        }).ToList();

        return views;
    }

    public async Task<Result<List<TourGuideViewModel>>> ListAll()
    {
        var tourGuides = await UnitOfWork.TourGuides.Query().ToListAsync();
        return tourGuides.Adapt<List<TourGuideViewModel>>();
    }
}