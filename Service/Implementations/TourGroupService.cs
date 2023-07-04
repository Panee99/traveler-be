using Data.EFCore;
using Data.Entities;
using Data.Enums;
using Mapster;
using Microsoft.EntityFrameworkCore;
using Service.Interfaces;
using Service.Models.Activity;
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

    public async Task<int> CountTravelers(Guid tourGroupId)
    {
        return await UnitOfWork.TourGroups
            .Query()
            .Where(e => e.Id == tourGroupId)
            .SelectMany(e => e.Travelers)
            .CountAsync();
    }

    public async Task<Result<TourGroupViewModel>> Get(Guid id)
    {
        var group = await UnitOfWork.TourGroups.FindAsync(id);
        if (group is null) return Error.NotFound();

        // return
        var view = group.Adapt<TourGroupViewModel>();
        view.TravelerCount = await CountTravelers(id);
        return view;
    }

    public async Task<Result<TourGroupViewModel>> Create(TourGroupCreateModel model)
    {
        var trip = await UnitOfWork.Trips.FindAsync(model.TripId);
        if (trip is null) return Error.NotFound();

        var group = UnitOfWork.TourGroups.Add(new TourGroup
        {
            TripId = trip.Id,
            GroupName = model.GroupName
        });

        await UnitOfWork.SaveChangesAsync();

        // return
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

        // return
        var view = group.Adapt<TourGroupViewModel>();
        view.TravelerCount = await CountTravelers(groupId);
        return view;
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
            .AsSplitQuery()
            .Where(e => e.Id == tourGroupId)
            .Include(e => e.TourGuide)
            .Include(e => e.Travelers)
            .SingleOrDefaultAsync();

        if (tourGroup is null) return Error.NotFound("Tour Group not found.");

        var members = tourGroup.Travelers.Select(traveler =>
        {
            var member = traveler.Adapt<UserViewModel>();
            member.AvatarUrl = _cloudStorageService.GetMediaLink(traveler.AvatarId);
            return member;
        }).ToList();

        var tourGuide = tourGroup.TourGuide;
        if (tourGuide != null)
        {
            var tourGuideMember = tourGuide.Adapt<UserViewModel>();
            tourGuideMember.AvatarUrl = _cloudStorageService.GetMediaLink(tourGuide.AvatarId);

            members.Add(tourGuideMember);
        }

        return members;
    }

    public async Task<Result<List<ActivityViewModel>>> ListActivities(Guid tourGroupId)
    {
        if (!await UnitOfWork.TourGroups.AnyAsync(e => e.Id == tourGroupId))
            return Error.NotFound();

        var attendanceActivities = UnitOfWork.AttendanceActivities.Query()
            .Include(x => x.Items)!
            .ThenInclude(x => x.User)
            .Where(x => x.TourGroupId == tourGroupId)
            .Select(x => new ActivityViewModel
                { Type = ActivityType.Attendance, Data = x, CreatedAt = (DateTime)x.CreatedAt! })
            .ToList();

        var customActivities = UnitOfWork.CustomActivities.Query()
            .Where(x => x.TourGroupId == tourGroupId)
            .Select(x => new ActivityViewModel
                { Type = ActivityType.Attendance, Data = x, CreatedAt = (DateTime)x.CreatedAt! })
            .ToList();

        var nextDestinationActivities = UnitOfWork.NextDestinationActivities.Query()
            .Where(x => x.TourGroupId == tourGroupId)
            .Select(x => new ActivityViewModel
                { Type = ActivityType.Attendance, Data = x, CreatedAt = (DateTime)x.CreatedAt! })
            .ToList();

        var activities = attendanceActivities.Concat(customActivities).Concat(nextDestinationActivities)
            .OrderBy(x => x.CreatedAt).ToList();

        return activities;
    }
}