using Data.EFCore;
using Data.Entities;
using Data.Enums;
using Mapster;
using Microsoft.EntityFrameworkCore;
using Service.Channels.Notification;
using Service.Interfaces;
using Service.Models.Attendance;
using Service.Models.AttendanceEvent;
using Shared.Helpers;
using Shared.ResultExtensions;

namespace Service.Implementations;

public class AttendanceEventService : BaseService, IAttendanceEventService
{
    private readonly INotificationJobQueue _notificationJobQueue;

    public AttendanceEventService(UnitOfWork unitOfWork,
        INotificationJobQueue notificationJobQueue) : base(unitOfWork)
    {
        _notificationJobQueue = notificationJobQueue;
    }

    public async Task<Result<AttendanceEventViewModel>> Create(AttendanceEventCreateModel model)
    {
        var tourGroup = await UnitOfWork.TourGroups.FindAsync(model.TourGroupId);
        if (tourGroup is null) return Error.NotFound("Tour Group not found");

        // Create attendance event
        var attendanceEvent = model.Adapt<AttendanceEvent>();
        attendanceEvent.CreatedAt = DateTimeHelper.VnNow();
        UnitOfWork.AttendanceEvents.Add(attendanceEvent);

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
                attendanceEvent.Name,
                NotificationType.AttendanceEvent
            ));

        // Return
        return attendanceEvent.Adapt<AttendanceEventViewModel>();
    }

    public async Task<Result> Delete(Guid id)
    {
        var attendanceEvent = await UnitOfWork.AttendanceEvents.FindAsync(id);
        if (attendanceEvent is null) return Error.NotFound();

        UnitOfWork.AttendanceEvents.Remove(attendanceEvent);
        await UnitOfWork.SaveChangesAsync();

        return Result.Success();
    }

    public async Task<Result<AttendanceViewModel>> CreateAttendance(Guid attendanceEventId, AttendanceCreateModel model)
    {
        if (!await UnitOfWork.AttendanceEvents.AnyAsync(e => e.Id == attendanceEventId))
            return Error.NotFound("Attendance event not found.");

        // check if user in right group
        var isUserInGroup = await UnitOfWork.AttendanceEvents
            .Query()
            .Where(evt => evt.Id == attendanceEventId)
            .Select(evt => evt.TourGroup)
            .SelectMany(group => group.Travelers)
            .AnyAsync(traveler => traveler.Id == model.TravelerId);

        if (!isUserInGroup) return Error.Conflict("User not in this group");

        // create 
        var attendance = new Attendance()
        {
            AttendanceEventId = attendanceEventId,
            TravelerId = model.TravelerId,
            Present = model.Present,
            Reason = model.Reason
        };

        UnitOfWork.Attendances.Add(attendance);
        await UnitOfWork.SaveChangesAsync();

        return attendance.Adapt<AttendanceViewModel>();
    }

    public async Task<Result<List<AttendanceViewModel>>> ListAttendances(Guid attendanceEventId)
    {
        if (!await UnitOfWork.AttendanceEvents.AnyAsync(e => e.Id == attendanceEventId))
            return Error.NotFound("Attendance event not found.");

        var attendances = await UnitOfWork.Attendances
            .Query()
            .Where(e => e.AttendanceEventId == attendanceEventId)
            .ToListAsync();

        return attendances.Adapt<List<AttendanceViewModel>>();
    }
}