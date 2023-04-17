namespace Service.Models.Booking;

public record BookingCreateModel
(
    Guid TourId,
    int AdultQuantity,
    int ChildrenQuantity,
    int InfantQuantity
);