using Data.EFCore;
using Data.EFCore.Repositories;
using Data.Entities;
using Data.Enums;
using Mapster;
using Microsoft.EntityFrameworkCore;
using Service.Interfaces;
using Service.Models.Account;
using Service.Models.Tour;
using Service.Models.TourGuide;
using Shared.Helpers;
using Shared.ResultExtensions;

namespace Service.Implementations;

public class TourGuideService : BaseService, ITourGuideService
{
    private readonly IRepository<TourGuide> _tourGuideRepo;
    private readonly ICloudStorageService _cloudStorageService;

    public TourGuideService(IUnitOfWork unitOfWork, ICloudStorageService cloudStorageService) : base(unitOfWork)
    {
        _cloudStorageService = cloudStorageService;
        _tourGuideRepo = unitOfWork.Repo<TourGuide>();
    }

    public async Task<Result<ProfileViewModel>> Create(TourGuideCreateModel model)
    {
        if (await _tourGuideRepo.AnyAsync(e => e.Phone == model.Phone))
            return Error.Conflict("Phone number already exist");

        if (await _tourGuideRepo.AnyAsync(e => e.Email == model.Email))
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
            Role = AccountRole.TourGuide,
            Status = AccountStatus.Active,
        };

        _tourGuideRepo.Add(tourGuide);

        await UnitOfWork.SaveChangesAsync();

        return tourGuide.Adapt<ProfileViewModel>();
    }

    public async Task<Result<List<TourFilterViewModel>>> ListAssignedTours(Guid tourGuideId)
    {
        if (!await _tourGuideRepo.AnyAsync(e => e.Id == tourGuideId))
            return Error.NotFound("Tour Guide not found.");

        var assignedTours = await _tourGuideRepo.Query()
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

    public async Task<Result<List<ProfileViewModel>>> ListAll()
    {
        var tourGuides = await _tourGuideRepo.Query().ToListAsync();
        return tourGuides.Adapt<List<ProfileViewModel>>();
    }
}