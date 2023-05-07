using Data.Enums;
using Service.Models.TourFlow;

namespace Service.Models.Tour;

public record TourUpdateModel
(
    string? Title,
    double? AdultPrice,
    double? ChildrenPrice,
    double? InfantPrice,
    string? Departure,
    string? Destination,
    DateTime? StartTime,
    DateTime? EndTime,
    int? MaxOccupancy,
    TourType? Type,
    string? Description,
    TourStatus? Status,
    List<TourFlowCreateModel>? TourFlows,
    Guid? ThumbnailId
);