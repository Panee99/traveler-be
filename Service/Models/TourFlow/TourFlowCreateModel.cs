namespace Service.Models.TourFlow;

public record TourFlowCreateModel
(
    float Longitude,
    float Latitude,
    DateTime ArrivalTime,
    string? Description
);