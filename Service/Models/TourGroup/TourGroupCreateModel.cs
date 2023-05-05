namespace Service.Models.TourGroup;

public record TourGroupCreateModel
(
    Guid TourId,
    string GroupName,
    int MaxOccupancy,
    Guid? TourGuide
);