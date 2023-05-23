using Service.Models.Passenger;

namespace Service.Models.Booking;

public record BookingCreateModel
(
    // Contact
    string ContactName,
    string ContactPhone,
    string? ContactEmail,
    string ContactAddress,
    string ContactCity,
    string ContactCountry,
    //
    Guid TourVariantId,
    int AdultQuantity,
    int ChildrenQuantity,
    int InfantQuantity,
    List<PassengerCreateModel> Passengers
);