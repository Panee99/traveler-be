namespace Service.Models.TourGroup;

public record TourGroupCreateModel
(
    Guid TourId,
    string GroupName
);