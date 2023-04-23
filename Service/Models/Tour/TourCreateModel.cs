using Data.Enums;
using Service.Models.Location;

namespace Service.Models.Tour;

public record TourCreateModel
(
    string Title,
    double Price,
    double ChildrenPrice,
    double BabyPrice,
    string Departure,
    string Destination,
    DateTime StartTime,
    DateTime EndTime,
    int MaxOccupancy,
    TourType Type,
    List<LocationCreateModel> Locations,
    string? Description
);