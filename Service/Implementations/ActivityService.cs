using Data.EFCore;
using Data.Entities.Activities;
using Data.Enums;
using Microsoft.EntityFrameworkCore;
using Service.Channels.Notification;
using Service.Commons.Mapping;
using Service.Interfaces;
using Service.Models.Activity;
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

    public async Task<Result> Create(PartialActivityModel model)
    {
        var (repo, tourGroupId, dataModel) = _destructurePartialActivityModel(model);

        if (tourGroupId == null || await UnitOfWork.TourGroups.FindAsync(tourGroupId) is not { } tourGroup)
            return Error.NotFound(DomainErrors.TourGroup.NotFound);

        repo.Add(dataModel);
        await UnitOfWork.SaveChangesAsync();

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
        else if (await UnitOfWork.NextDestinationActivities.FindAsync(id) is { } nextDestinationActivity)
        {
            UnitOfWork.NextDestinationActivities.Remove(nextDestinationActivity);
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
            { Type: ActivityType.NextDestination } => (UnitOfWork.NextDestinationActivities,
                model.NextDestinationActivity?.TourGroupId, model.NextDestinationActivity),
            _ => throw new ArgumentOutOfRangeException(nameof(model), model, null)
        };
    }
}