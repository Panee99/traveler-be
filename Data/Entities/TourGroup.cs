namespace Data.Entities;

public class TourGroup
{
    public Guid Id { get; set; }

    public Guid TourId { get; set; }

    public Guid? TourGuideId { get; set; }

    public string GroupName { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public Tour Tour { get; set; } = null!;
    public TourGuide? TourGuide { get; set; } = null!;
}