using Data.Enums;

namespace Service.Models.Booking;

public record BookingViewModel
{
    public Guid Id { get; set; }
    public Guid TourVariantId { get; set; }
    public Guid TravelerId { get; set; }
    public int AdultQuantity { get; set; }
    public int ChildrenQuantity { get; set; }
    public int InfantQuantity { get; set; }
    public double AdultPrice { get; set; }
    public double ChildrenPrice { get; set; }
    public double InfantPrice { get; set; }
    public BookingStatus Status { get; set; }
    public DateTime ExpireAt { get; set; }
    public DateTime Timestamp { get; set; }
    public ICollection<PassengerViewModel> Passengers { get; set; } = new List<PassengerViewModel>();
}

public record PassengerViewModel
{
    public string Name = null!;
    public string? Phone;
    public string? Address;
    public string? Gender;
    public string? Country;
    public string? Passport;
}