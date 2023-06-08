using Data.EFCore;
using Data.Entities;
using Mapster;
using Microsoft.EntityFrameworkCore;
using Service.Interfaces;
using Service.Models.AttendanceEvent;
using Service.Models.TourGroup;
using Service.Models.User;
using Shared.ResultExtensions;

namespace Service.Implementations;

public class TourGroupService : BaseService, ITourGroupService
{
    private readonly ICloudStorageService _cloudStorageService;

    public TourGroupService(UnitOfWork unitOfWork, ICloudStorageService cloudStorageService) : base(unitOfWork)
    {
        _cloudStorageService = cloudStorageService;
    }

    public async Task<Result<TourGroupViewModel>> Get(Guid id)
    {
        var group = await UnitOfWork.TourGroups.FindAsync(id);
        if (group is null) return Error.NotFound();

        return group.Adapt<TourGroupViewModel>();
    }

    public async Task<Result<TourGroupViewModel>> Create(TourGroupCreateModel model)
    {
        var tourVariant = await UnitOfWork.TourVariants.FindAsync(model.TourVariantId);
        if (tourVariant is null) return Error.NotFound();

        var group = UnitOfWork.TourGroups.Add(new TourGroup
        {
            TourVariantId = tourVariant.Id,
            GroupName = model.GroupName
        });

        await UnitOfWork.SaveChangesAsync();

        return group.Adapt<TourGroupViewModel>();
    }

    public async Task<Result<TourGroupViewModel>> Update(Guid groupId, TourGroupUpdateModel model)
    {
        var group = await UnitOfWork.TourGroups.FindAsync(groupId);
        if (group is null) return Error.NotFound("Tour group not found.");

        if (model.GroupName != null) group.GroupName = model.GroupName;

        if (model.TourGuideId != null)
        {
            var tourGuide = await UnitOfWork.TourGuides.FindAsync(model.TourGuideId);
            if (tourGuide is null) return Error.NotFound("Tour guide not found.");
            group.TourGuide = tourGuide;
        }

        UnitOfWork.TourGroups.Update(group);

        await UnitOfWork.SaveChangesAsync();

        return group.Adapt<TourGroupViewModel>();
    }

    public async Task<Result> Delete(Guid groupId)
    {
        var group = await UnitOfWork.TourGroups.FindAsync(groupId);
        if (group is null) return Error.NotFound();

        UnitOfWork.TourGroups.Remove(group);
        await UnitOfWork.SaveChangesAsync();
        return Result.Success();
    }

    public async Task<Result> AddTravelers(Guid tourGroupId, ICollection<Guid> travelerIds)
    {
        if (!await UnitOfWork.TourGroups.AnyAsync(e => e.Id == tourGroupId))
            return Error.NotFound();

        var records = travelerIds.Select(travelerId => new TravelerInTourGroup
        {
            TravelerId = travelerId,
            TourGroupId = tourGroupId
        });

        // Check all traveler ids
        var existTravelers = await UnitOfWork.Travelers
            .Query()
            .Where(e => travelerIds.Contains(e.Id))
            .Select(e => e.Id).ToListAsync();

        var nonExistTravelers = travelerIds.Except(existTravelers).ToList();

        if (nonExistTravelers.Count != 0)
            return Error.NotFound(nonExistTravelers.Select(id => id.ToString()).ToArray());

        UnitOfWork.TravelersInTourGroups.AddRange(records);

        await UnitOfWork.SaveChangesAsync();
        return Result.Success();
    }

    public async Task<Result> RemoveTravelers(Guid tourGroupId, List<Guid> travelerIds)
    {
        if (!await UnitOfWork.TourGroups.AnyAsync(e => e.Id == tourGroupId))
            return Error.NotFound();

        var records = await UnitOfWork.TravelersInTourGroups
            .Query()
            .Where(e => e.TourGroupId == tourGroupId && travelerIds.Contains(e.TravelerId))
            .ToListAsync();

        UnitOfWork.TravelersInTourGroups.RemoveRange(records);
        await UnitOfWork.SaveChangesAsync();

        return Result.Success();
    }

    public async Task<Result<List<UserViewModel>>> ListMembers(Guid tourGroupId)
    {
        var tourGroup = await UnitOfWork.TourGroups
            .Query()
            .Where(e => e.Id == tourGroupId)
            .Include(e => e.TourGuide)
            .Include(e => e.Travelers)
            .SingleOrDefaultAsync();

        if (tourGroup is null) return Error.NotFound("Tour Group not found.");

        var members = tourGroup.Travelers.Select(traveler =>
        {
            var member = traveler.Adapt<UserViewModel>();
            member.AvatarUrl = traveler.AvatarId is null
                ? null
                : _cloudStorageService.GetMediaLink(traveler.AvatarId.Value);
            return member;
        }).ToList();

        var tourGuide = tourGroup.TourGuide;
        if (tourGuide != null)
        {
            var tourGuideMember = tourGuide.Adapt<UserViewModel>();
            tourGuideMember.AvatarUrl = tourGuide.AvatarId is null
                ? null
                : _cloudStorageService.GetMediaLink(tourGuide.AvatarId.Value);

            members.Add(tourGuideMember);
        }

        return members;
    }

    public async Task<Result<List<AttendanceEventViewModel>>> ListAttendanceEvents(Guid tourGroupId)
    {
        if (!await UnitOfWork.TourGroups.AnyAsync(e => e.Id == tourGroupId))
            return Error.NotFound();

        var events = await UnitOfWork.AttendanceEvents
            .Query()
            .Where(e => e.TourGroupId == tourGroupId)
            .ToListAsync();

        return events.Adapt<List<AttendanceEventViewModel>>();
    }
}