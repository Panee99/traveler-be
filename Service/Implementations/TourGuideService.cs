using Data.EFCore;
using Mapster;
using Microsoft.EntityFrameworkCore;
using Service.Interfaces;
using Service.Models.Tour;
using Service.Models.TourGroup;
using Service.Models.TourGuide;
using Shared.ResultExtensions;

namespace Service.Implementations;

public class TourGuideService : BaseService, ITourGuideService
{
    private readonly ICloudStorageService _cloudStorageService;

    public TourGuideService(UnitOfWork unitOfWork,
        ICloudStorageService cloudStorageService) : base(unitOfWork)
    {
        _cloudStorageService = cloudStorageService;
    }

    public async Task<Result<List<TourGroupViewModel>>> ListAssignedGroups(Guid tourGuideId)
    {
        if (!await UnitOfWork.TourGuides.AnyAsync(e => e.Id == tourGuideId))
            return Error.NotFound(DomainErrors.TourGuide.NotFound);

        var groupResults = await UnitOfWork.TourGuides
            .Query()
            .AsSplitQuery()
            .Where(guide => guide.Id == tourGuideId)
            .SelectMany(guide => guide.TourGroups)
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
            })
            .ToList();
    }

    public async Task<Result<TourGuideViewModel>> UpdateContacts(Guid tourGuideId, ContactsUpdateModel model)
    {
        var tourGuide = await UnitOfWork.TourGuides.FindAsync(tourGuideId);
        if (tourGuide is null) return Error.NotFound();

        model.Adapt(tourGuide);
        UnitOfWork.TourGuides.Update(tourGuide);
        await UnitOfWork.SaveChangesAsync();

        var view = tourGuide.Adapt<TourGuideViewModel>();
        view.AvatarUrl = _cloudStorageService.GetMediaLink(tourGuide.AvatarId);

        return view;
    }
}