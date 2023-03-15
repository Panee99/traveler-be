namespace Data.Entities;

public class Location
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public string Address { get; set; } = null!;

    public double Longitude { get; set; }

    public double Latitude { get; set; }

    public virtual ICollection<LocationTag> LocationTags { get; set; } = new List<LocationTag>();

    public virtual ICollection<Waypoint> Waypoints { get; set; } = new List<Waypoint>();

    public virtual ICollection<LocationAttachment> LocationAttachments { get; set; } = new List<LocationAttachment>();
}