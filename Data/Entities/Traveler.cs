using Data.Enums;

namespace Data.Entities;

public class Traveler : User
{
    //
    public string? BankName { get; set; }

    public string? BankNumber { get; set; }

    public DateTime? Birthday { get; set; }

    public string? Address { get; set; }

    public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();

    public virtual ICollection<TourGroup> TourGroups { get; set; } = new List<TourGroup>();
}