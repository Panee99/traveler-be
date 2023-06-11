namespace Data.Entities;

public class AttendanceEvent
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public Guid TourGroupId { get; set; }

    public TourGroup TourGroup { get; set; } = null!;

    public DateTime CreatedAt { get; set; }
    
    public virtual ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();
}