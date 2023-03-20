namespace Data.Entities;

public class Waypoint
{
    public Guid TourId { get; set; }

    public Guid LocationId { get; set; }

    public DateTime ArrivalAt { get; set; }

    public bool IsPrimary { get; set; }

    public string? Description { get; set; }

    public virtual Location Location { get; set; } = null!;

    public virtual Tour Tour { get; set; } = null!;
}