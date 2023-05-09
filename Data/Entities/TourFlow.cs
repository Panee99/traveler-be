namespace Data.Entities;

public class TourFlow
{
    public Guid Id { get; set; }

    public Guid TourId { get; set; }

    public virtual Tour Tour { get; set; } = null!;

    public float Longitude { get; set; }

    public float Latitude { get; set; }

    public DateTime ArrivalTime { get; set; }
    
    public string? Description { get; set; } = null!;
}