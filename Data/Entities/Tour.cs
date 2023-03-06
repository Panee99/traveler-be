using System;
using System.Collections.Generic;

namespace Data.Entities;

public partial class Tour
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

    public virtual ICollection<Booking> Bookings { get; } = new List<Booking>();

    public virtual ICollection<IncurredCost> IncurredCosts { get; } = new List<IncurredCost>();

    public virtual ICollection<TourCarousel> TourCarousels { get; } = new List<TourCarousel>();

    public virtual ICollection<TourDiscount> TourDiscounts { get; } = new List<TourDiscount>();

    public virtual ICollection<Waypoint> Waypoints { get; } = new List<Waypoint>();
}
