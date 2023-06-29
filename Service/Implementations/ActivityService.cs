using Data.EFCore;
using Data.Entities;
using Data.Enums;
using Mapster;
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
    private readonly INotificationJobQueue _notificationJobQueue;

    public ActivityService(UnitOfWork unitOfWork,
        INotificationJobQueue notificationJobQueue) : base(unitOfWork)
    {
        _notificationJobQueue = notificationJobQueue;
    }
    
    public async Task<Result<ActivityViewModel>> Create(ActivityCreateModel model)
    {
        var tourGroup = await UnitOfWork.TourGroups.FindAsync(model.TourGroupId);
        if (tourGroup is null) return Error.NotFound("Tour Group not found");

        // Create activity
        var activity = model.Adapt<Activity>();
        activity.Status = ActivityStatus.Pending;
        activity.CreatedAt = DateTimeHelper.VnNow();
        UnitOfWork.Activities.Add(activity);

        await UnitOfWork.SaveChangesAsync();

        // Notifications
        var receiverIds = await UnitOfWork.TourGroups
            .Query()
            .Where(group => group.Id == model.TourGroupId)
            .SelectMany(group => group.Travelers)
            .Select(traveler => traveler.Id)
            .ToListAsync();

        if (tourGroup.TourGuideId != null) receiverIds.Add(tourGroup.TourGuideId.Value);

        await _notificationJobQueue.EnqueueAsync(
            new NotificationJob(
                receiverIds,
                "Tour Guide",
                activity.Title,
                activity.Type switch
                {
                    ActivityType.Attendance => NotificationType.AttendanceActivity,
                    _ => throw new ArgumentOutOfRangeException()
                }
            ));

        // Return
        return activity.Adapt<ActivityViewModel>();
    }

    public async Task<Result> Delete(Guid id)
    {
        var activity = await UnitOfWork.Activities.FindAsync(id);
        if (activity is null) return Error.NotFound();

        UnitOfWork.Activities.Remove(activity);
        await UnitOfWork.SaveChangesAsync();

        return Result.Success();
    }
    
    public async Task<Result<ActivityViewModel>> Update(Guid activityId, ActivityUpdateModel model)
    {
        var activity = await UnitOfWork.Activities.FindAsync(activityId);
        if (activity is null) return Error.NotFound();

        model.AdaptIgnoreNull(activity);
        UnitOfWork.Activities.Update(activity);
        await UnitOfWork.SaveChangesAsync();

        return activity.Adapt<ActivityViewModel>();
    }
}