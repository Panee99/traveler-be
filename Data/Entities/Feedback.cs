namespace Data.Entities;

public class Feedback
{
    public Guid Id { get; set; }

    public string? Comment { get; set; }

    public byte Rating { get; set; }

    public Guid TourId { get; set; }

    public Tour Tour { get; set; } = null!;

    public Guid TravelerId { get; set; }

    public Traveler Traveler { get; set; } = null!;

    public DateTime Timestamp { get; set; }
}