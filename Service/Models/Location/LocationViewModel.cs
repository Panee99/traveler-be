using Data.Enums;

namespace Service.Models.Location;

public record LocationViewModel
{
    public Guid Id;
    public double Latitude;
    public double Longitude;
    public Vehicle Vehicle;
    public DateTime ArrivalTime;
}