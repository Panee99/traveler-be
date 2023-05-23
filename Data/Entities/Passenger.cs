using Data.Enums;

namespace Data.Entities;

public class Passenger
{
    public Guid Id { get; set; }

    public Guid BookingId { get; set; }

    public Booking Booking { get; set; } = null!;
    
    public string Name { get; set; } = null!;

    public string? Phone { get; set; }

    public string? Address { get; set; }

    public Gender? Gender { get; set; }
    
    public string? Country { get; set; }
    
    public string? Passport { get; set; }
    
    
}