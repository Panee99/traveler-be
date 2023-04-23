using Data.EFCore;
using Data.EFCore.Repositories;
using Data.Entities;
using Mapster;
using Microsoft.EntityFrameworkCore;
using Service.Interfaces;
using Service.Models.Tour;
using Shared.ResultExtensions;

namespace Service.Implementations;

public class TourGuideService : BaseService, ITourGuideService
{
    private readonly IRepository<TourGuide> _tourGuidRepo;
    private readonly ICloudStorageService _cloudStorageService;

    public TourGuideService(IUnitOfWork unitOfWork, ICloudStorageService cloudStorageService) : base(unitOfWork)
    {
        _cloudStorageService = cloudStorageService;
        _tourGuidRepo = unitOfWork.Repo<TourGuide>();
    }

    public async Task<Result<List<TourFilterViewModel>>> ListAssignedTours(Guid tourGuideId)
    {
        if (!await _tourGuidRepo.AnyAsync(e => e.Id == tourGuideId))
            return Error.NotFound("Tour Guide not found.");

        var assignedTours = await _tourGuidRepo.Query()
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
}