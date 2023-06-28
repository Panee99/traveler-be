namespace Data.Entities;

public class AttendanceDetail
{
    public Guid Id { get; set; }

    public Guid ActivityId { get; set; }

    public Guid TravelerId { get; set; }

    public bool Present { get; set; }

    public string? Reason { get; set; }

    public Traveler Traveler { get; set; } = null!;

    public Activity Activity { get; set; } = null!;
}