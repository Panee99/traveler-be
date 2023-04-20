using Data.Enums;

namespace Service.Models.Location;

public record LocationUpdateModel
(
    long? Longitude,
    long? Latitude,
    Vehicle? Vehicle,
    DateTime? ArrivalTime
);