namespace Service.Models.TourFlow;

public record TourFlowCreateModel
(
    Guid TourId,
    Guid LocationId,
    DateTime ArrivalAt,
    bool IsPrimary,
    string? Description
);