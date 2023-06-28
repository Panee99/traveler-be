namespace Service.Models.TourGroup;

public record TourGroupCreateModel
(
    Guid TripId,
    string GroupName,
    int MaxOccupancy,
    Guid? TourGuide
);