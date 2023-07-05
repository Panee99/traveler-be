using Data.EFCore;
using Data.EFCore.Repositories;
using Data.Entities;
using Data.Entities.Activities;
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

    public async Task<Result> Create(ActivityCreateModel model)
    {
        var (repo, tourGroupId, dataModel) = model switch
        {
            { Type: ActivityType.Attendance } => (UnitOfWork.AttendanceActivities,
                model.AttendanceActivity?.TourGroupId, model.AttendanceActivity),
            { Type: ActivityType.Custom } => (UnitOfWork.CustomActivities,
                model.CustomActivity?.TourGroupId, model.CustomActivity),
            { Type: ActivityType.NextDestination } => (UnitOfWork.NextDestinationActivities as dynamic,
                model.NextDestinationActivity?.TourGroupId, model.NextDestinationActivity as dynamic),
            _ => throw new ArgumentOutOfRangeException()
        };

        if (tourGroupId == null || await UnitOfWork.TourGroups.FindAsync(tourGroupId) is not { } tourGroup)
            return Error.NotFound("Tour Group not found");

        repo.Add(dataModel);
        await UnitOfWork.SaveChangesAsync();

        if (model.Type is not ActivityType.Attendance) return Result.Success();
        
        // Notifications
        var receiverIds = await UnitOfWork.TourGroups
            .Query()
            .Where(group => group.Id == tourGroupId)
            .SelectMany(group => group.Travelers)
            .Select(traveler => traveler.Id)
            .ToListAsync();

        if (tourGroup.TourGuideId != null) receiverIds.Add(tourGroup.TourGuideId.Value);

        await _notificationJobQueue.EnqueueAsync(
            new NotificationJob(
                receiverIds,
                "Tour Guide",
                dataModel?.Title,
                NotificationType.AttendanceActivity
            ));

        // Return
        return Result.Success();
    }

    public async Task<Result> Delete(Guid id)
    {
        var deleted = false;
        if (await UnitOfWork.AttendanceActivities.FindAsync(id) is { } attendanceActivity)
        {
            UnitOfWork.AttendanceActivities.Remove(attendanceActivity);
            deleted = true;
        }
        else if (await UnitOfWork.CustomActivities.FindAsync(id) is { } customActivity)
        {
            UnitOfWork.CustomActivities.Remove(customActivity);
            deleted = true;
        }
        else if (await UnitOfWork.IncurredCostActivities.FindAsync(id) is { } incurredCostActivity)
        {
            UnitOfWork.IncurredCostActivities.Remove(incurredCostActivity);
            deleted = true;
        }
        else if (await UnitOfWork.NextDestinationActivities.FindAsync(id) is { } nextDestinationActivity)
        {
            UnitOfWork.NextDestinationActivities.Remove(nextDestinationActivity);
            deleted = true;
        }

        if (!deleted) return Error.NotFound();

        await UnitOfWork.SaveChangesAsync();

        return Result.Success();
    }

    public async Task<Result> Update(AttendanceActivity model)
    {
        if (model.Id == null || await UnitOfWork.AttendanceActivities.FindAsync(model.Id) is not { } activity)
            return Error.NotFound();

        model.AdaptIgnoreNull(activity);
        UnitOfWork.AttendanceActivities.Update(activity);
        await UnitOfWork.SaveChangesAsync();

        return Result.Success();
    }

    public async Task<Result> Update(CustomActivity model)
    {
        if (model.Id == null || await UnitOfWork.CustomActivities.FindAsync(model.Id) is not { } activity)
            return Error.NotFound();

        model.AdaptIgnoreNull(activity);
        UnitOfWork.CustomActivities.Update(activity);
        await UnitOfWork.SaveChangesAsync();

        return Result.Success();
    }

    public async Task<Result> Update(IncurredCostActivity model)
    {
        if (model.Id == null || await UnitOfWork.IncurredCostActivities.FindAsync(model.Id) is not { } activity)
            return Error.NotFound();

        model.AdaptIgnoreNull(activity);
        UnitOfWork.IncurredCostActivities.Update(activity);
        await UnitOfWork.SaveChangesAsync();

        return Result.Success();
    }

    public async Task<Result> Update(NextDestinationActivity model)
    {
        if (model.Id == null || await UnitOfWork.NextDestinationActivities.FindAsync(model.Id) is not { } activity)
            return Error.NotFound();

        model.AdaptIgnoreNull(activity);
        UnitOfWork.NextDestinationActivities.Update(activity);
        await UnitOfWork.SaveChangesAsync();

        return Result.Success();
    }
}