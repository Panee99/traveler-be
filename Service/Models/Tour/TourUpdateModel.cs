using Data.Enums;

namespace Service.Models.Tour;

public record TourUpdateModel
(
    string? Title,
    string? Code,
    double? Price,
    double? ChildrenPrice,
    double? BabyPrice,
    string? Departure,
    string? Destination,
    DateTime? StartTime,
    DateTime? EndTime,
    int? MaxOccupancy,
    TourType? Type,
    string? Description,
    TourStatus? Status
);