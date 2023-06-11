namespace Service.Models.TourGroup;

public record TourGroupCreateModel
(
    Guid TourVariantId,
    string GroupName,
    int MaxOccupancy,
    Guid? TourGuide
);