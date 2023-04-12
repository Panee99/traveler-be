namespace Data.Entities;

public class Location
{
    public Guid Id { get; set; }

    public Guid TourId { get; set; }

    public double Longitude { get; set; }

    public double Latitude { get; set; }

    public virtual Tour Tour { get; set; } = null!;
    
    public DateTime ArrivalTime { get; set; }
    public DateTime CreatedAt { get; set; }
}