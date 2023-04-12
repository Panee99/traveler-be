namespace Service.Models.TourFlow;

public record TourFlowUpdateModel
(
    DateTime? ArrivalAt,
    bool? IsPrimary,
    string? Description
);