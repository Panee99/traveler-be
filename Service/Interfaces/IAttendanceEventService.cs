using Service.Models.Attendance;
using Service.Models.AttendanceEvent;
using Shared.ResultExtensions;

namespace Service.Interfaces;

public interface IAttendanceEventService
{
    Task<Result<AttendanceEventViewModel>> Create(AttendanceEventCreateModel model);

    Task<Result> Delete(Guid id);

    Task<Result<AttendanceViewModel>> CreateAttendance(Guid attendanceEventId, AttendanceCreateModel model);
    
    Task<Result<List<AttendanceViewModel>>> ListAttendances(Guid attendanceEventId);
}