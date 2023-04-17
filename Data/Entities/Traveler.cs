using Data.Enums;

namespace Data.Entities;

public class Traveler : Account
{
    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public DateTime? Birthday { get; set; }

    public Gender Gender { get; set; }

    public string? Address { get; set; }

    public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    
    public virtual ICollection<TravelerInTour> TravelerInTours { get; set; } = new List<TravelerInTour>();
}