namespace Service.Models.Booking;

public record BookingCreateModel
(
    Guid TourVariantId,
    int AdultQuantity,
    int ChildrenQuantity,
    int InfantQuantity
);