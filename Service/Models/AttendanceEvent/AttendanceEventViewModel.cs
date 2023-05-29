namespace Service.Models.AttendanceEvent;

public record AttendanceEventViewModel
{
    public string Name = null!;
    public Guid TourGroupId;
    public DateTime CreatedAt;
}