using Data.EFCore;
using Mapster;
using Microsoft.EntityFrameworkCore;
using Service.Interfaces;
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
            // Filter out deleted Tour and Trip
            .Where(group => group.Trip.DeletedById == null && 
                            group.Trip.Tour.DeletedById == null)
            .Include(group => group.Trip)
            .ThenInclude(trip => trip.Tour)
            .ThenInclude(tour => tour.Thumbnail)
            .Select(group => new { Group = group, TravelerCount = group.Travelers.Count })
            .ToListAsync();

        // return
        return groupResults.Select(groupResult =>
            {
                var view = groupResult.Group.Adapt<TourGroupViewModel>();
                view.Trip!.Tour!.ThumbnailUrl =
                    _cloudStorageService.GetMediaLink(groupResult.Group.Trip.Tour.Thumbnail?.FileName);
                view.TravelerCount = groupResult.TravelerCount;
                return view;
            })
            .ToList();
    }

    public async Task<Result<TourGuideViewModel>> UpdateContacts(Guid tourGuideId, ContactsUpdateModel model)
    {
        var tourGuide = await UnitOfWork.TourGuides
            .Query()
            .Where(guide => guide.Id == tourGuideId)
            .Include(guide => guide.Avatar)
            .FirstOrDefaultAsync();

        if (tourGuide is null) return Error.NotFound();

        model.Adapt(tourGuide);
        UnitOfWork.TourGuides.Update(tourGuide);
        await UnitOfWork.SaveChangesAsync();

        var view = tourGuide.Adapt<TourGuideViewModel>();
        view.AvatarUrl = _cloudStorageService.GetMediaLink(tourGuide.Avatar?.FileName);

        return view;
    }
}