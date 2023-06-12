using Data.EFCore;
using Data.Enums;
using Mapster;
using Microsoft.EntityFrameworkCore;
using Service.Interfaces;
using Service.Models.Tour;
using Service.Models.TourGroup;
using Shared.ResultExtensions;

namespace Service.Implementations;

public class TourGuideService : BaseService, ITourGuideService
{
    private readonly ICloudStorageService _cloudStorageService;
    private readonly ITourGroupService _tourGroupService;

    public TourGuideService(UnitOfWork unitOfWork,
        ICloudStorageService cloudStorageService,
        ITourGroupService tourGroupService) : base(unitOfWork)
    {
        _cloudStorageService = cloudStorageService;
        _tourGroupService = tourGroupService;
    }

    public async Task<Result<List<TourViewModel>>> ListAssignedTours(Guid tourGuideId)
    {
        if (!await UnitOfWork.TourGuides.AnyAsync(e => e.Id == tourGuideId))
            return Error.NotFound("Tour Guide not found.");

        var assignedTours = await UnitOfWork.TourGuides.Query()
            .SelectMany(guide => guide.TourGroups)
            .Select(group => group.TourVariant)
            .Select(variant => variant.Tour)
            .ToListAsync();

        var views = assignedTours.Select(e =>
        {
            var view = e.Adapt<TourViewModel>();
            view.ThumbnailUrl = _cloudStorageService.GetMediaLink(e.ThumbnailId);

            return view;
        }).ToList();

        return views;
    }

    public async Task<Result<List<TourGroupViewModel>>> ListAssignedGroups(Guid tourGuideId)
    {
        if (!await UnitOfWork.TourGuides.AnyAsync(e => e.Id == tourGuideId))
            return Error.NotFound("Tour Guide not found.");

        var assignedGroups = await UnitOfWork.TourGuides
            .Query()
            .AsSplitQuery()
            .Where(guide => guide.Id == tourGuideId)
            .SelectMany(guide => guide.TourGroups)
            .Include(group => group.TourVariant)
            .ThenInclude(variant => variant.Tour)
            .ToListAsync();


        // return
        var views = await Task.WhenAll(assignedGroups.Select(async group =>
        {
            var tour = group.TourVariant.Tour;
            var view = group.Adapt<TourGroupViewModel>();
            view.TourVariant!.Tour!.ThumbnailUrl = _cloudStorageService.GetMediaLink(tour.ThumbnailId);
            view.TravelerCount = await _tourGroupService.CountTravelers(group.Id);
            return view;
        }));

        return views.ToList();
    }

    public async Task<Result<TourGroupViewModel>> GetCurrentAssignedTourGroup(Guid tourGuideId)
    {
        if (!await UnitOfWork.TourGuides.AnyAsync(e => e.Id == tourGuideId))
            return Error.NotFound("Tour Guide not found.");

        var currentGroup = await UnitOfWork.TourGuides
            .Query()
            .AsSplitQuery()
            .Where(guide => guide.Id == tourGuideId)
            .SelectMany(guide => guide.TourGroups)
            .Include(group => group.TourVariant)
            .ThenInclude(variant => variant.Tour)
            .Where(group => group.TourVariant.Status != TourVariantStatus.Ended &&
                            group.TourVariant.Status != TourVariantStatus.Canceled)
            .FirstOrDefaultAsync();

        if (currentGroup is null) return Error.NotFound("No current tour group assigned");

        var tour = currentGroup.TourVariant.Tour;
        var view = currentGroup.Adapt<TourGroupViewModel>();
        view.TourVariant!.Tour!.ThumbnailUrl = _cloudStorageService.GetMediaLink(tour.ThumbnailId);
        view.TravelerCount = await _tourGroupService.CountTravelers(view.Id);

        return view;
    }
}