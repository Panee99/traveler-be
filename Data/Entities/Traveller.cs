namespace Data.Entities;

public class Traveller
{
    public Guid Id { get; set; }

    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public DateTime? Birthday { get; set; }

    public string? AvatarUrl { get; set; }

    public string Gender { get; set; } = null!;

    public string? Address { get; set; }

    public Guid AccountId { get; set; }

    public virtual Account Account { get; set; } = null!;

    public virtual ICollection<Booking> Bookings { get; } = new List<Booking>();
}
