namespace Service.Models.AttendanceEvent;

public record AttendanceEventCreateModel
(
    string Name,
    Guid TourGroupId
);