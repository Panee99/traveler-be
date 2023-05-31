using Data.EFCore;
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

    public TourGuideService(UnitOfWork unitOfWork, ICloudStorageService cloudStorageService) : base(unitOfWork)
    {
        _cloudStorageService = cloudStorageService;
    }

    public async Task<Result<List<TourFilterViewModel>>> ListAssignedTours(Guid tourGuideId)
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
            var view = e.Adapt<TourFilterViewModel>();
            if (e.ThumbnailId != null)
                view.ThumbnailUrl = _cloudStorageService.GetMediaLink(e.ThumbnailId.Value);

            return view;
        }).ToList();

        return views;
    }

    public async Task<Result<List<TourGroupViewModel>>> ListAssignedGroups(Guid tourGuideId)
    {
        if (!await UnitOfWork.TourGuides.AnyAsync(e => e.Id == tourGuideId))
            return Error.NotFound("Tour Guide not found.");
        
        var assignedGroups = await UnitOfWork.TourGuides.Query()
            .SelectMany(guide => guide.TourGroups)
            .ToListAsync();

        return assignedGroups.Adapt<List<TourGroupViewModel>>();
    }
}