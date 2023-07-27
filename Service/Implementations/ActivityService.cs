using Data.EFCore;
using Data.Entities.Activities;
using Data.Enums;
using FireSharp.Interfaces;
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
    private readonly IFirebaseClient _firebaseClient;
    private static string AttendanceKey = "attendances";
    private readonly INotificationService _notificationService;

    public ActivityService(UnitOfWork unitOfWork, IFirebaseClient firebaseClient,
        INotificationService notificationService) : base(unitOfWork)
    {
        _firebaseClient = firebaseClient;
        _notificationService = notificationService;
    }

    // public async Task<Result> Create(AttendanceActivity model)
    // {
    //     var tourGroup = await UnitOfWork.TourGroups.FindAsync(model.TourGroupId);
    //     if (tourGroup is null) return Error.NotFound("Tour Group not found");
    //
    //     UnitOfWork.AttendanceActivities.Add(model);
    //
    //     await UnitOfWork.SaveChangesAsync();
    //
    //     // Notifications
    //     var receiverIds = await UnitOfWork.TourGroups
    //         .Query()
    //         .Where(group => group.Id == model.TourGroupId)
    //         .SelectMany(group => group.Travelers)
    //         .Select(traveler => traveler.Id)
    //         .ToListAsync();
    //
    //     if (tourGroup.TourGuideId != null) receiverIds.Add(tourGroup.TourGuideId.Value);
    //
    //     await _notificationJobQueue.EnqueueAsync(
    //         new NotificationJob(
    //             receiverIds,
    //             "Tour Guide",
    //             model.Title,
    //             NotificationType.AttendanceActivity
    //         ));
    //
    //     // Return
    //     return Result.Success();
    // }

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
                        AttendanceAt = DateTimeHelper.VnNow()
                    }).ToList();
            }

            dataModel = attendanceActivity;
        }

        var entity = repo.Add(dataModel);

        await UnitOfWork.SaveChangesAsync();

        // Realtime Database
        if (entity is AttendanceActivity attendanceEntity)
            _insertAttendanceIntoRealtimeDatabase(attendanceEntity.Id!.Value, attendanceEntity.Items!);

        // Notifications
        var receiverIds = await UnitOfWork.TourGroups
            .Query()
            .Where(group => group.Id == tourGroupId)
            .SelectMany(group => group.Travelers)
            .Select(traveler => traveler.Id)
            .ToListAsync();

        if (tourGroup.TourGuideId != null) receiverIds.Add(tourGroup.TourGuideId.Value);

        await _notificationService.EnqueueNotification(
            new NotificationJob(
                receiverIds,
                "Tour Guide",
                dataModel?.Title,
                NotificationType.AttendanceActivity
            ));

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
        else if (await UnitOfWork.CustomActivities.FindAsync(id) is { } customActivity)
        {
            customActivity.IsDeleted = true;
            UnitOfWork.CustomActivities.Update(customActivity);
            deleted = true;
        }
        else if (await UnitOfWork.CheckInActivities.FindAsync(id) is { } checkInActivity)
        {
            checkInActivity.IsDeleted = true;
            UnitOfWork.CheckInActivities.Update(checkInActivity);
            deleted = true;
        }

        if (!deleted) return Error.NotFound();

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

        return Result.Success();
    }

    private (dynamic repo, Guid? tourGroupId, dynamic? dataModel) _destructurePartialActivityModel(
        PartialActivityModel model)
    {
        return model switch
        {
            { Type: ActivityType.Attendance } => (UnitOfWork.AttendanceActivities,
                model.AttendanceActivity?.TourGroupId, model.AttendanceActivity),
            { Type: ActivityType.Custom } => (UnitOfWork.CustomActivities,
                model.CustomActivity?.TourGroupId, model.CustomActivity),
            { Type: ActivityType.CheckIn } => (UnitOfWork.CheckInActivities,
                model.CheckInActivity?.TourGroupId, model.CheckInActivity),
            _ => throw new ArgumentOutOfRangeException(nameof(model), model, null)
        };
    }

    private void _insertAttendanceIntoRealtimeDatabase(Guid attendanceId, ICollection<AttendanceItem> items)
    {
        var path = $"{AttendanceKey}/{attendanceId}";
        _firebaseClient.Set(path, items);
    }
}