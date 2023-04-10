namespace Service.Models.Location;

public record LocationCreateModel
(
    long Longitude,
    long Latitude,
    DateTime ArrivalTime
);