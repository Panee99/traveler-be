namespace Data.Entities;

public class TourFlow
{
    public Guid Id { get; set; }

    public Guid TourId { get; set; }

    public virtual Tour Tour { get; set; } = null!;

    public double Longitude { get; set; }

    public double Latitude { get; set; }

    public DateTime ArrivalTime { get; set; }

    public string? Description { get; set; } = null!;
}