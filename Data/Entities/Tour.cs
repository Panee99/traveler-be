using Data.Enums;

namespace Data.Entities;

public class Tour
{
    public Guid Id { get; set; }

    public string Code { get; set; } = null!;

    public string Title { get; set; } = null!;

    public double Price { get; set; }

    public string? Description { get; set; }

    public string Type { get; set; } = null!;

    public DateTime StartTime { get; set; }

    public DateTime EndTime { get; set; }

    public string Vehicle { get; set; } = null!;

    public int NumberOfTickets { get; set; }

    public TourStatus Status { get; set; }

    public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();

    public virtual ICollection<IncurredCost> IncurredCosts { get; set; } = new List<IncurredCost>();

    public virtual ICollection<TourDiscount> TourDiscounts { get; set; } = new List<TourDiscount>();

    public virtual ICollection<Waypoint> Waypoints { get; set; } = new List<Waypoint>();
}
