namespace Service.Models.Trip;

public record TripCreateModel
(
    Guid TourId,
    int AdultPrice,
    int ChildrenPrice,
    int InfantPrice,
    DateTime StartTime,
    DateTime EndTime
);