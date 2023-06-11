namespace Service.Models.AttendanceEvent;

public record AttendanceEventViewModel
{
    public Guid Id;
    public string Name = null!;
    public Guid TourGroupId;
    public DateTime CreatedAt;
}