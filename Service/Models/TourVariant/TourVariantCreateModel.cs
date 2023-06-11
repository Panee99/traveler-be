namespace Service.Models.TourVariant;

public record TourVariantCreateModel
(
    Guid TourId,
    int AdultPrice,
    int ChildrenPrice,
    int InfantPrice,
    DateTime StartTime,
    DateTime EndTime
);