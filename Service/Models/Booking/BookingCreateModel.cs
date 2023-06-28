using Service.Models.Passenger;

namespace Service.Models.Booking;

public record BookingCreateModel
(
    Guid TripId,
    int AdultQuantity,
    int ChildrenQuantity,
    int InfantQuantity,
    List<PassengerCreateModel> Passengers
);