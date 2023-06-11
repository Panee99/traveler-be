namespace Data.Entities;

public class Attendance
{
    public Guid Id { get; set; }

    public Guid AttendanceEventId { get; set; }

    public AttendanceEvent AttendanceEvent { get; set; } = null!;

    public Guid TravelerId { get; set; }

    public Traveler Traveler { get; set; } = null!;

    public bool Present { get; set; }

    public string? Reason { get; set; }
}