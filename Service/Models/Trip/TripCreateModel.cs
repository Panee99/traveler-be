namespace Service.Models.Trip;

public record TripCreateModel
(
    Guid TourId,
    DateTime StartTime,
    DateTime EndTime
);