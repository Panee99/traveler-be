using Data.Enums;

namespace Service.Models.Booking;

public record BookingViewModel
{
    public Guid Id { get; set; }
    public Guid TourId { get; set; }
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
}