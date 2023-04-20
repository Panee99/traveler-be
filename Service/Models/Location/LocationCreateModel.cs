using Data.Enums;

namespace Service.Models.Location;

public record LocationCreateModel
(
    long Longitude,
    long Latitude,
    Vehicle Vehicle,
    DateTime ArrivalTime
);