namespace Data.Entities;

public class Traveler : User
{
    public string? Address { get; set; }

    public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();

    public virtual ICollection<TourGroup> TourGroups { get; set; } = new List<TourGroup>();
}