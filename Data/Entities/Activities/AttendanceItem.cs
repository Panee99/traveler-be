namespace Data.Entities.Activities;

public class AttendanceItem
{
    public Guid AttendanceId { get; set; }

    public AttendanceActivity? Attendance { get; set; }

    public Guid UserId { get; set; }

    public User? User { get; set; }

    public bool Present { get; set; } = false;

    public string Reason { get; set; } = string.Empty;
}