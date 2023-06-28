﻿using Data.EFCore;
using Data.Entities;
using Data.Enums;
using Mapster;
using Microsoft.EntityFrameworkCore;
using Service.Channels.Notification;
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

    // public async Task<Result<AttendanceViewModel>> CreateAttendance(Guid attendanceEventId, AttendanceCreateModel model)
    // {
    //     if (!await UnitOfWork.AttendanceEvents.AnyAsync(e => e.Id == attendanceEventId))
    //         return Error.NotFound("AttendanceDetail event not found.");
    //
    //     // check if user in right group
    //     var isUserInGroup = await UnitOfWork.AttendanceEvents
    //         .Query()
    //         .Where(evt => evt.Id == attendanceEventId)
    //         .Select(evt => evt.TourGroup)
    //         .SelectMany(group => group.Travelers)
    //         .AnyAsync(traveler => traveler.Id == model.TravelerId);
    //
    //     if (!isUserInGroup) return Error.Conflict("User not in this group");
    //
    //     // create 
    //     var attendance = new AttendanceDetail()
    //     {
    //         AttendanceEventId = attendanceEventId,
    //         TravelerId = model.TravelerId,
    //         Present = model.Present,
    //         Reason = model.Reason
    //     };
    //
    //     UnitOfWork.Attendances.Add(attendance);
    //     await UnitOfWork.SaveChangesAsync();
    //
    //     return attendance.Adapt<AttendanceViewModel>();
    // }

    // public async Task<Result<List<AttendanceViewModel>>> ListAttendances(Guid attendanceEventId)
    // {
    //     if (!await UnitOfWork.AttendanceEvents.AnyAsync(e => e.Id == attendanceEventId))
    //         return Error.NotFound("AttendanceDetail event not found.");
    //
    //     var attendances = await UnitOfWork.Attendances
    //         .Query()
    //         .Where(e => e.AttendanceEventId == attendanceEventId)
    //         .ToListAsync();
    //
    //     return attendances.Adapt<List<AttendanceViewModel>>();
    // }
}