namespace Data.Entities;

public class LocationTag
{
    public Guid LocationId { get; set; }

    public Guid TagId { get; set; }

    public Location Location { get; set; } = null!;

    public Tag Tag { get; set; } = null!;
}