namespace Data.Entities;

public class TravelerInTourGroup
{
    public Guid TravelerId { get; set; }

    public Traveler Traveler { get; set; } = null!;

    public Guid TourGroupId { get; set; }

    public TourGroup TourGroup { get; set; } = null!;
}