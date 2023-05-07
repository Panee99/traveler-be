namespace Service.Models.TourFlow;

public record TourFlowCreateModel
(
    string Description,
    int Sequence,
    DateTime? From,
    DateTime? To
);