using Data.Enums;

namespace Data.Entities;

public class Trip
{
    public Guid Id { get; set; }

    public Guid TourId { get; set; }

    public Tour Tour { get; set; } = null!;

    public string Code { get; set; } = null!;

    public int AdultPrice { get; set; }

    public int ChildrenPrice { get; set; }

    public int InfantPrice { get; set; }

    public DateTime StartTime { get; set; }

    public DateTime EndTime { get; set; }

    public TripStatus Status { get; set; }

    public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();

    public virtual ICollection<TourGroup> TourGroups { get; set; } = new List<TourGroup>();
}