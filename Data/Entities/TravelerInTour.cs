namespace Data.Entities;

public class TravelerInTour
{
    public Guid Id { get; set; }

    public Guid TravelerId { get; set; }

    public Guid TourId { get; set; }

    public Traveler Traveler { get; set; } = null!;

    public Tour Tour { get; set; } = null!;

    public Guid TourGroupId { get; set; }

    public TourGroup TourGroup { get; set; } = null!;
}