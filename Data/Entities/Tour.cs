using Data.Enums;

namespace Data.Entities;

public class Tour
{
    public Guid Id { get; set; }

    public string Title { get; set; } = null!;

    public string Departure { get; set; } = null!;

    public string Destination { get; set; } = null!;

    public TourType Type { get; set; }

    public DateTime CreatedAt { get; set; }

    public string Duration { get; set; } = null!;

    public string? Description { get; set; }

    public string? Policy { get; set; }

    public Guid? ThumbnailId { get; set; }

    public Attachment? Thumbnail { get; set; }

    public virtual ICollection<Trip> Trips { get; set; } = new List<Trip>();

    public virtual ICollection<Schedule> Schedules { get; set; } = new List<Schedule>();

    public virtual ICollection<TourImage> TourCarousel { get; set; } = new List<TourImage>();
}