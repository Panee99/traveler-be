namespace Service.Models.TourFlow;

public record TourFlowCreateModel
(
    double Longitude,
    double Latitude,
    DateTime ArrivalTime,
    string? Description
);