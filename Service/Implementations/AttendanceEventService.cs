using Data.EFCore;
using Data.Entities;
using Mapster;
using Microsoft.EntityFrameworkCore;
using Service.Interfaces;
using Service.Models.Attendance;
using Service.Models.AttendanceEvent;
using Shared.Channels.Notification;
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
        // Create attendance event
        var attendanceEvent = model.Adapt<AttendanceEvent>();
        attendanceEvent.CreatedAt = DateTimeHelper.VnNow();
        UnitOfWork.AttendanceEvents.Add(attendanceEvent);

        await UnitOfWork.SaveChangesAsync();

        // Send notification
        var travelerIds = await UnitOfWork.TourGroups
            .Query()
            .Where(group => group.Id == model.TourGroupId)
            .SelectMany(group => group.Travelers)
            .Select(traveler => traveler.Id)
            .ToListAsync();

        var fcmTokens = await UnitOfWork.FcmTokens
            .Query()
            .Where(e => travelerIds.Contains(e.UserId))
            .Select(e => e.Token)
            .ToListAsync();

        await _notificationJobQueue.EnqueueAsync(
            new NotificationJob(fcmTokens, "Attendance event",
                $"A new attendance event opened: {attendanceEvent.Name}"));

        // Save notification
        // TODO

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