namespace Service.Models.Attendance;

public record AttendanceCreateModel
(
    Guid TravelerId,
    bool Present,
    string? Reason
);