using Data.Enums;

namespace Data.Entities;

public class Tour
{
    public Guid Id { get; set; }

    public string Code { get; set; } = null!;

    public string Title { get; set; } = null!;

    public double Price { get; set; }

    public double ChildrenPrice { get; set; }

    public double BabyPrice { get; set; }

    public string Departure { get; set; } = null!;

    public string Destination { get; set; } = null!;

    public DateTime StartTime { get; set; }

    public DateTime EndTime { get; set; }

    public string Vehicle { get; set; } = null!;

    public int MaxOccupancy { get; set; }

    public TourType Type { get; set; }

    public TourStatus Status { get; set; }

    public DateTime CreatedAt { get; set; }

    public string? Description { get; set; }

    public Guid? ThumbnailId { get; set; }

    public Attachment? Thumbnail { get; set; }

    public virtual ICollection<TourGroup> TourGroups { get; set; } = new List<TourGroup>();

    public virtual ICollection<Location> TourLocations { get; set; } = new List<Location>();

    // public virtual ICollection<TourDiscount> TourDiscounts { get; set; } = new List<TourDiscount>();
}