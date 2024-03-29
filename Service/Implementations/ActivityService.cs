using Data.EFCore;
using Data.Entities.Activities;
using Data.Enums;
using FireSharp.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Service.Channels.Notification;
using Service.Commons.Mapping;
using Service.Interfaces;
using Service.Models.Activity;
using Shared.Helpers;
using Shared.ResultExtensions;

namespace Service.Implementations;

public class ActivityService : BaseService, IActivityService
{
    private const string AttendanceKey = "attendances";
    private readonly IFirebaseClient _firebaseClient;
    private readonly ICloudStorageService _cloudStorageService;
    private readonly INotificationService _notificationService;

    public ActivityService(UnitOfWork unitOfWork,
        IFirebaseClient firebaseClient,
        ICloudStorageService cloudStorageService,
        INotificationService notificationService, IHttpContextAccessor httpContextAccessor
    ) : base(unitOfWork, httpContextAccessor)
    {
        _firebaseClient = firebaseClient;
        _cloudStorageService = cloudStorageService;
        _notificationService = notificationService;
    }

    public async Task<Result<string>> Create(PartialActivityModel model)
    {
        var (repo, tourGroupId, dataModel) = _destructurePartialActivityModel(model);

        if (tourGroupId == null || await UnitOfWork.TourGroups.FindAsync(tourGroupId) is not { } tourGroup)
            return Error.NotFound(DomainErrors.TourGroup.NotFound);

        // Activity is attendance
        if (dataModel is AttendanceActivity attendanceActivity)
        {
            if (attendanceActivity.Items == null || attendanceActivity.Items.Count == 0)
            {
                // Append members to attendance activity
                attendanceActivity.Items = UnitOfWork.TourGroups.Query().Include(x => x.Travelers)
                    .FirstOrDefault(x => x.Id == tourGroupId)?.Travelers.Select(x => new AttendanceItem
                    {
                        Present = false,
                        Reason = string.Empty,
                        UserId = x.Id,
                        AttendanceAt = DateTimeHelper.VnNow(),
                        LastUpdateAt = DateTimeHelper.VnNow(),
                    }).ToList();
            }

            dataModel = attendanceActivity;
        }

        var entity = repo.Add(dataModel);

        await UnitOfWork.SaveChangesAsync();

        // Sync Realtime Database
        if (entity is AttendanceActivity attendanceEntity)
            _syncAttendanceWithRealtimeDatabase(attendanceEntity.Id!.Value);
        // _insertAttendanceIntoRealtimeDatabase(attendanceEntity.Id!.Value,
        //     new FirebaseAttendanceModel
        //         { Items = attendanceEntity.Items!.Adapt<ICollection<FirebaseAttendanceItem>>(), Title = "" });

        if (entity is AttendanceActivity activity)
        {
            // Notifications
            var receiverIds = await UnitOfWork.TourGroups
                .Query()
                .Where(group => group.Id == tourGroupId)
                .SelectMany(group => group.Travelers)
                .Select(traveler => traveler.Id)
                .ToListAsync();

            if (tourGroup.TourGuideId != null) receiverIds.Add(tourGroup.TourGuideId.Value);

            var tripId = await UnitOfWork.TourGroups.Query()
                .Where(group => group.Id == activity.TourGroupId)
                .Select(group => group.TripId)
                .FirstOrDefaultAsync();

            await _notificationService.EnqueueNotification(
                new NotificationJob(tripId, receiverIds,
                    NotificationType.AttendanceActivity, null));
        }

        // Return
        return entity.Id.ToString();
    }

    public async Task<Result> Delete(Guid id)
    {
        var deleted = false;
        if (await UnitOfWork.AttendanceActivities.FindAsync(id) is { } attendanceActivity)
        {
            attendanceActivity.IsDeleted = true;
            UnitOfWork.AttendanceActivities.Update(attendanceActivity);
            deleted = true;
        }
        else if (await UnitOfWork.CheckInActivities.FindAsync(id) is { } checkInActivity)
        {
            checkInActivity.IsDeleted = true;
            UnitOfWork.CheckInActivities.Update(checkInActivity);
            deleted = true;
        }
        else if (await UnitOfWork.IncurredCostActivities.FindAsync(id) is { } incurredCostActivity)
        {
            incurredCostActivity.IsDeleted = true;
            UnitOfWork.IncurredCostActivities.Update(incurredCostActivity);
            deleted = true;
        }

        if (!deleted) return Error.NotFound();

        await UnitOfWork.SaveChangesAsync();

        return Result.Success();
    }

    public async Task<Result> DeleteDraft(Guid id)
    {
        var deleted = false;
        if (await UnitOfWork.AttendanceActivities.FindAsync(id) is { } attendanceActivity)
        {
            UnitOfWork.AttendanceActivities.Remove(attendanceActivity);
            deleted = true;
        }
        else if (await UnitOfWork.CheckInActivities.FindAsync(id) is { } checkInActivity)
        {
            UnitOfWork.CheckInActivities.Remove(checkInActivity);
            deleted = true;
        }
        else if (await UnitOfWork.IncurredCostActivities.FindAsync(id) is { } incurredCostActivity)
        {
            UnitOfWork.IncurredCostActivities.Remove(incurredCostActivity);
            deleted = true;
        }

        if (!deleted) return Error.NotFound();

        // remove from realtime database
        _removeAttendanceFromRealtimeDatabase(id);

        await UnitOfWork.SaveChangesAsync();

        return Result.Success();
    }

    public async Task<Result> Update(PartialActivityModel model)
    {
        var (repo, _, dataModel) = _destructurePartialActivityModel(model);

        var activity = await repo.FindAsync(dataModel?.Id);
        if (dataModel == null || activity == null)
            return Error.NotFound(DomainErrors.Activity.NotFound);

        (dataModel as object).CustomAdaptIgnoreNull(activity as object);

        repo.Update(activity);
        await UnitOfWork.SaveChangesAsync();

        // Sync Realtime Database
        if (model.Type == ActivityType.Attendance && activity != null)
        {
            _syncAttendanceWithRealtimeDatabase(activity!.Id);
        }

        return Result.Success();
    }

    public async Task<Result> Attend(Guid code)
    {
        if (CurrentUser == null) return Error.Authentication();
        var userId = CurrentUser.Id;

        var attendanceItem =
            await UnitOfWork.AttendanceItems.Query()
                .FirstOrDefaultAsync(x => x.AttendanceActivityId == code && x.UserId == userId);

        if (attendanceItem == null) return Error.NotFound();

        attendanceItem.Present = true;
        UnitOfWork.AttendanceItems.Update(attendanceItem);
        await UnitOfWork.SaveChangesAsync();
        _syncAttendanceWithRealtimeDatabase(code);

        return Result.Success();
    }

    public async Task<Result<ActivityViewModel>> Get(Guid id)
    {
        //! support only incurred cost activity
        var activity = await UnitOfWork.IncurredCostActivities.Query()
            .Include(x => x.Image)
            .Where(x => x.Id == id)
            .Where(x => x.IsDeleted == false)
            .Select(x => new ActivityViewModel
                { Type = ActivityType.IncurredCost, Data = x, CreatedAt = (DateTime)x.CreatedAt! })
            .FirstOrDefaultAsync();
        // set incurred costs image url
        if (activity is not { Data: IncurredCostActivity incurredCostActivity }) return Error.NotFound();

        incurredCostActivity.ImageUrl = _cloudStorageService.GetMediaLink(activity.Data.Image?.FileName);

        return activity;
    }

    private (dynamic repo, Guid? tourGroupId, dynamic? dataModel) _destructurePartialActivityModel(
        PartialActivityModel model)
    {
        return model switch
        {
            { Type: ActivityType.Attendance } => (UnitOfWork.AttendanceActivities,
                model.AttendanceActivity?.TourGroupId, model.AttendanceActivity),
            { Type: ActivityType.CheckIn } => (UnitOfWork.CheckInActivities,
                model.CheckInActivity?.TourGroupId, model.CheckInActivity),
            { Type: ActivityType.IncurredCost } => (UnitOfWork.IncurredCostActivities,
                model.IncurredCostActivity?.TourGroupId, model.IncurredCostActivity),
            _ => throw new ArgumentOutOfRangeException(nameof(model), model, null)
        };
    }

    private void _removeAttendanceFromRealtimeDatabase(Guid attendanceId)
    {
        var path = $"{AttendanceKey}/{attendanceId}";
        _firebaseClient.Delete(path);
    }

    private void _syncAttendanceWithRealtimeDatabase(Guid id)
    {
        var path = $"{AttendanceKey}/{id}";
        var activity = UnitOfWork.AttendanceActivities
            .Query()
            .Include(x => x.Items)
            .Include(x => x.TourGroup).ThenInclude(x => x!.Travelers)
            .FirstOrDefault(x => x.Id == id);

        if (activity == null) return;

        var data = new FirebaseAttendanceModel
        {
            Items = activity.Items!.Select(item =>
            {
                var traveler = activity.TourGroup!.Travelers.FirstOrDefault(traveler => traveler.Id == item.UserId);
                return new FirebaseAttendanceItem
                {
                    AttendanceAt = item.AttendanceAt,
                    Present = item.Present,
                    Reason = item.Reason,
                    UserId = item.UserId,
                    Name = traveler != null ? traveler.FirstName + " " + traveler.LastName : string.Empty,
                    AvatarUrl = _cloudStorageService.GetMediaLink(traveler?.AvatarId.ToString()) ?? string.Empty,
                    LastUpdateAt = item.LastUpdateAt
                };
            }).ToDictionary(x => x.UserId, x => x),
            Title = activity.Title ?? string.Empty
        };

        _firebaseClient.Update(path, data);
    }
}