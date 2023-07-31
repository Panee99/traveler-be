using Data.EFCore;
using Data.Enums;
using Google.Cloud.Firestore;
using Mapster;
using Microsoft.EntityFrameworkCore;
using Service.Interfaces;
using Service.Models.Activity;
using Service.Models.TourGroup;
using Service.Models.User;
using Shared.Helpers;
using Shared.ResultExtensions;

namespace Service.Implementations;

public class TourGroupService : BaseService, ITourGroupService
{
    private readonly ICloudStorageService _cloudStorageService;
    private readonly FirestoreDb _firestoreDb;

    public TourGroupService(UnitOfWork unitOfWork, ICloudStorageService cloudStorageService, FirestoreDb firestoreDb) :
        base(unitOfWork)
    {
        _cloudStorageService = cloudStorageService;
        _firestoreDb = firestoreDb;
    }

    public async Task<int> CountTravelers(Guid tourGroupId)
    {
        return await UnitOfWork.TourGroups
            .Query()
            .Where(e => e.Id == tourGroupId)
            .SelectMany(e => e.Travelers)
            .CountAsync();
    }

    public async Task<Result> Start(Guid id)
    {
        var tourGroup = await UnitOfWork.TourGroups.FindAsync(id);
        if (tourGroup is null) return Error.NotFound(DomainErrors.TourGroup.NotFound);

        if (tourGroup.Status != TourGroupStatus.Prepare)
            return Error.Conflict("Tour Group status must be 'Prepare' to Start");

        tourGroup.Status = TourGroupStatus.Ongoing;
        UnitOfWork.TourGroups.Update(tourGroup);
        await UnitOfWork.SaveChangesAsync();

        return Result.Success();
    }

    public async Task<Result> End(Guid id)
    {
        var tourGroup = await UnitOfWork.TourGroups.FindAsync(id);
        if (tourGroup is null) return Error.NotFound(DomainErrors.TourGroup.NotFound);

        if (tourGroup.Status != TourGroupStatus.Ongoing)
            return Error.Conflict("Tour Group status must be 'Ongoing' to End");

        tourGroup.Status = TourGroupStatus.Ended;
        UnitOfWork.TourGroups.Update(tourGroup);
        await UnitOfWork.SaveChangesAsync();

        return Result.Success();
    }

    public async Task<Result> Cancel(Guid id)
    {
        var tourGroup = await UnitOfWork.TourGroups.FindAsync(id);
        if (tourGroup is null) return Error.NotFound(DomainErrors.TourGroup.NotFound);

        if (tourGroup.Status is TourGroupStatus.Ended or TourGroupStatus.Canceled)
            return Error.Conflict("Cannot cancel 'Ended' or 'Canceled' Tour Group");

        tourGroup.Status = TourGroupStatus.Canceled;
        UnitOfWork.TourGroups.Update(tourGroup);
        await UnitOfWork.SaveChangesAsync();

        return Result.Success();
    }

    public async Task<Result> SendEmergency(Guid tourGroupId, Guid senderId, EmergencyRequestModel model)
    {
        var messagesRef = _firestoreDb
            .Collection("groups")
            .Document(tourGroupId.ToString())
            .Collection("messages");

        if (!await UnitOfWork.TourGroups.AnyAsync(e => e.Id == tourGroupId))
            return Error.NotFound(DomainErrors.TourGroup.NotFound);

        var sender = await UnitOfWork.Users.FindAsync(senderId);
        if (sender is null) return Error.NotFound(DomainErrors.User.NotFound);

        var content = $"{sender.FirstName} {sender.LastName} sent Emergency request.\n" +
                      $"https://www.google.com/maps/@{model.Latitude},{model.Longitude}";

        var message = new
        {
            Content = content,
            SenderId = sender.Id.ToString(),
            Timestamp = DateTimeHelper.VnNow().ToString("yyyy-MM-ddTHH:mm:ss.ff"),
            Type = "text",
        };

        var result = await messagesRef.AddAsync(message);
        Console.WriteLine(result.Id);

        return Result.Success();
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

    // public async Task<Result<TourGroupViewModel>> Update(Guid groupId, TourGroupUpdateModel model)
    // {
    //     var group = await UnitOfWork.TourGroups.FindAsync(groupId);
    //     if (group is null) return Error.NotFound(DomainErrors.TourGroup.NotFound);
    //
    //     if (model.GroupName != null) group.GroupName = model.GroupName;
    //
    //     if (model.TourGuideId != null)
    //     {
    //         var tourGuide = await UnitOfWork.TourGuides.FindAsync(model.TourGuideId);
    //         if (tourGuide is null) return Error.NotFound(DomainErrors.TourGuide.NotFound);
    //         group.TourGuide = tourGuide;
    //     }
    //
    //     UnitOfWork.TourGroups.Update(group);
    //
    //     await UnitOfWork.SaveChangesAsync();
    //
    //     // return
    //     var view = group.Adapt<TourGroupViewModel>();
    //     view.TravelerCount = await CountTravelers(groupId);
    //     return view;
    // }

    public async Task<Result> Delete(Guid groupId)
    {
        var group = await UnitOfWork.TourGroups.FindAsync(groupId);
        if (group is null) return Error.NotFound();

        UnitOfWork.TourGroups.Remove(group);
        await UnitOfWork.SaveChangesAsync();
        return Result.Success();
    }

    public async Task<Result<List<UserViewModel>>> ListMembers(Guid tourGroupId)
    {
        var tourGroup = await UnitOfWork.TourGroups
            .Query()
            .AsSplitQuery()
            .Where(e => e.Id == tourGroupId)
            .Include(e => e.TourGuide).ThenInclude(guide => guide.Avatar)
            .Include(e => e.Travelers).ThenInclude(traveler => traveler.Avatar)
            .SingleOrDefaultAsync();

        if (tourGroup is null) return Error.NotFound(DomainErrors.TourGroup.NotFound);

        var members = tourGroup.Travelers.Select(traveler =>
        {
            var member = traveler.Adapt<UserViewModel>();
            member.AvatarUrl = _cloudStorageService.GetMediaLink(traveler.Avatar?.FileName);
            return member;
        }).ToList();

        var tourGuide = tourGroup.TourGuide;
        if (tourGuide != null)
        {
            var tourGuideMember = tourGuide.Adapt<UserViewModel>();
            tourGuideMember.AvatarUrl = _cloudStorageService.GetMediaLink(tourGuide.Avatar?.FileName);

            members.Add(tourGuideMember);
        }

        return members;
    }

    public async Task<Result<List<ActivityViewModel>>> ListActivities(Guid tourGroupId)
    {
        if (!await UnitOfWork.TourGroups.AnyAsync(e => e.Id == tourGroupId))
            return Error.NotFound();

        var attendanceActivities = UnitOfWork.AttendanceActivities.Query()
            .Where(x => x.TourGroupId == tourGroupId)
            .Where(x => x.IsDeleted == false)
            .Select(x => new ActivityViewModel
                { Type = ActivityType.Attendance, Data = x, CreatedAt = (DateTime)x.CreatedAt! })
            .ToList();

        var customActivities = UnitOfWork.CustomActivities.Query()
            .Where(x => x.TourGroupId == tourGroupId)
            .Where(x => x.IsDeleted == false)
            .Select(x => new ActivityViewModel
                { Type = ActivityType.Custom, Data = x, CreatedAt = (DateTime)x.CreatedAt! })
            .ToList();

        var checkInActivities = UnitOfWork.CheckInActivities.Query()
            .Where(x => x.TourGroupId == tourGroupId)
            .Where(x => x.IsDeleted == false)
            .Select(x => new ActivityViewModel
                { Type = ActivityType.CheckIn, Data = x, CreatedAt = (DateTime)x.CreatedAt! })
            .ToList();

        var activities = attendanceActivities.Concat(customActivities).Concat(checkInActivities)
            .OrderBy(x => x.CreatedAt).ToList();

        return activities;
    }
}