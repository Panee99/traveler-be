using Data.Enums;

namespace Data.Entities;

public class Tour
{
    public Guid Id { get; set; }

    public string Code { get; set; } = null!;

    public string Title { get; set; } = null!;

    public int AdultPrice { get; set; }

    public int ChildrenPrice { get; set; }

    public int InfantPrice { get; set; }

    public string Departure { get; set; } = null!;

    public string Destination { get; set; } = null!;

    public DateTime StartTime { get; set; }

    public DateTime EndTime { get; set; }

    public int MaxOccupancy { get; set; }

    public TourType Type { get; set; }

    public TourStatus Status { get; set; }

    public DateTime CreatedAt { get; set; }

    public string? Description { get; set; }

    public Guid? ThumbnailId { get; set; }

    public Attachment? Thumbnail { get; set; }

    public virtual ICollection<TourGroup> TourGroups { get; set; } = new List<TourGroup>();

    public virtual ICollection<Location> TourFlow { get; set; } = new List<Location>();

    public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();

    public virtual ICollection<TravelerInTour> TravelerInTours { get; set; } = new List<TravelerInTour>();

    // public virtual ICollection<TourDiscount> TourDiscounts { get; set; } = new List<TourDiscount>();
}