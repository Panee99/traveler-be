using Data.EFCore;
using Data.Enums;
using Google.Cloud.Firestore;
using Mapster;
using Microsoft.EntityFrameworkCore;
using Service.Channels.Notification;
using Service.Interfaces;
using Service.Models.Activity;
using Service.Models.TourGroup;
using Service.Models.User;
using Shared.Helpers;
using Shared.ResultExtensions;

namespace Service.Implementations;

public class TourGroupService : BaseService, ITourGroupService
{
    private readonly FirestoreDb _firestoreDb;
    private readonly ICloudStorageService _cloudStorageService;
    private readonly INotificationService _notificationService;

    public TourGroupService(UnitOfWork unitOfWork, ICloudStorageService cloudStorageService, FirestoreDb firestoreDb,
        INotificationService notificationService) :
        base(unitOfWork)
    {
        _cloudStorageService = cloudStorageService;
        _firestoreDb = firestoreDb;
        _notificationService = notificationService;
    }

    public async Task<int> CountTravelers(Guid tourGroupId)
    {
        return await UnitOfWork.TourGroups
            .Query()
            .Where(e => e.Id == tourGroupId)
            .SelectMany(e => e.Travelers)
            .CountAsync();
    }

    public async Task<Result> End(Guid id)
    {
        var tourGroup = await UnitOfWork.TourGroups
            .Query()
            .Where(e => e.Id == id)
            .Include(e => e.Trip)
            .FirstOrDefaultAsync();

        if (tourGroup is null) return Error.NotFound(DomainErrors.TourGroup.NotFound);

        if (tourGroup.Status != TourGroupStatus.Active)
            return Error.Conflict("Tour Group status must be 'Active' to End");

        // if (tourGroup.Trip.EndTime < DateTimeHelper.VnNow().Date)
        //     return Error.Conflict($"Cannot end until {tourGroup.Trip.EndTime}");

        tourGroup.Status = TourGroupStatus.Ended;
        UnitOfWork.TourGroups.Update(tourGroup);
        await UnitOfWork.SaveChangesAsync();

        return Result.Success();
    }

    public async Task<Result> SendEmergency(Guid tourGroupId, Guid senderId, EmergencyRequestModel model)
    {
        var group = await UnitOfWork.TourGroups.Query()
            .Where(e => e.Id == tourGroupId)
            .Include(e => e.Travelers)
            .FirstOrDefaultAsync();

        if (group is null) return Error.NotFound(DomainErrors.TourGroup.NotFound);

        var sender = await UnitOfWork.Users.FindAsync(senderId);
        if (sender is null) return Error.NotFound(DomainErrors.User.NotFound);

        // Send emergency message to group chat
        var messagesRef = _firestoreDb
            .Collection("groups")
            .Document(tourGroupId.ToString())
            .Collection("messages");

        if (!await UnitOfWork.TourGroups.AnyAsync(e => e.Id == tourGroupId))
            return Error.NotFound(DomainErrors.TourGroup.NotFound);

        var content = $"{sender.FirstName} {sender.LastName} sent Emergency request.\n" +
                      $"https://www.google.com/maps/@{model.Latitude},{model.Longitude}";

        var message = new
        {
            Content = content,
            SenderId = sender.Id.ToString(),
            Timestamp = DateTimeHelper.VnNow().ToString("yyyy-MM-ddTHH:mm:ss.ff"),
            Type = "custom",
        };

        await messagesRef.AddAsync(message);

        // Send via Notification
        var userIds = group.Travelers.Select(t => t.Id).ToList();
        if (group.TourGuideId != null) userIds.Add(group.TourGuideId.Value);

        await _notificationService.EnqueueNotification(new NotificationJob(
            userIds,
            NotificationType.Emergency,
            sender.AvatarId,
            $"{sender.FirstName} {sender.LastName}"
        ));

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
            member.DependantId = UnitOfWork.TravelersInTourGroups.Query()
                .Where(e => e.TravelerId == member.Id &&
                            e.TourGroupId == tourGroupId)
                .Select(e => e.DependantId)
                .FirstOrDefaultAsync().Result;

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

        var checkInActivities = UnitOfWork.CheckInActivities.Query()
            .Where(x => x.TourGroupId == tourGroupId)
            .Where(x => x.IsDeleted == false)
            .Select(x => new ActivityViewModel
                { Type = ActivityType.CheckIn, Data = x, CreatedAt = (DateTime)x.CreatedAt! })
            .ToList();

        var incurredCostActivities = UnitOfWork.IncurredCostActivities.Query()
            .Where(x => x.TourGroupId == tourGroupId)
            .Where(x => x.IsDeleted == false)
            .Select(x => new ActivityViewModel
                { Type = ActivityType.IncurredCost, Data = x, CreatedAt = (DateTime)x.CreatedAt! })
            .ToList();

        var activities = attendanceActivities.Concat(checkInActivities).Concat(incurredCostActivities)
            .OrderByDescending(x => x.CreatedAt).ToList();

        return activities;
    }
}