namespace Data.Entities;

public class TourGroup
{
    public Guid Id { get; set; }

    public Guid TourVariantId { get; set; }

    public Guid? TourGuideId { get; set; }

    public string GroupName { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public TourVariant TourVariant { get; set; } = null!;

    public int MaxOccupancy { get; set; }

    public TourGuide? TourGuide { get; set; } = null!;

    public virtual ICollection<Traveler> Travelers { get; set; } = new List<Traveler>();

    public virtual ICollection<AttendanceEvent> AttendanceEvents { get; set; } = new List<AttendanceEvent>();
}