using Data.Enums;

namespace Data.Entities;

public class Tour
{
    public Guid Id { get; set; }

    public string Title { get; set; } = null!;

    public string Departure { get; set; } = null!;

    public string Destination { get; set; } = null!;

    public int MaxOccupancy { get; set; }

    public TourType Type { get; set; }

    public TourStatus Status { get; set; }

    public DateTime CreatedAt { get; set; }

    public string Duration { get; set; } = null!;

    public string? Description { get; set; }

    public string? Policy { get; set; }

    public Guid? ThumbnailId { get; set; }

    public Attachment? Thumbnail { get; set; }

    public virtual ICollection<TourVariant> TourVariants { get; set; } = new List<TourVariant>();

    public virtual ICollection<Schedule> Schedules { get; set; } = new List<Schedule>();

    public virtual ICollection<TourImage> TourCarousel { get; set; } = new List<TourImage>();

    // public virtual ICollection<TourDiscount> TourDiscounts { get; set; } = new List<TourDiscount>();
}