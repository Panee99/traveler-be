using Data.Enums;

namespace Service.Models.Tour;

public record TourCreateModel
(
    string Title,
    string Code,
    double Price,
    double ChildrenPrice,
    double BabyPrice,
    string Departure,
    string Destination,
    DateTime StartTime,
    DateTime EndTime,
    string Vehicle,
    int MaxOccupancy,
    TourType Type,
    string? Description
);