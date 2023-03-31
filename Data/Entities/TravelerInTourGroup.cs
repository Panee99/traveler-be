namespace Data.Entities;

public class TravelerInTourGroup
{
    public Guid TravelerId { get; set; }

    public Guid TourGroupId { get; set; }

    public Traveler Traveler { get; set; } = null!;

    public TourGroup TourGroup { get; set; } = null!;
}