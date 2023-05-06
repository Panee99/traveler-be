using Data.Enums;
using Service.Models.Location;

namespace Service.Models.Tour;

public record TourCreateModel
(
    string Title,
    double AdultPrice,
    double ChildrenPrice,
    double InfantPrice,
    string Departure,
    string Destination,
    DateTime StartTime,
    DateTime EndTime,
    int MaxOccupancy,
    TourType Type,
    string? Description,
    List<LocationCreateModel> TourFlow,
    Guid? ThumbnailId
);