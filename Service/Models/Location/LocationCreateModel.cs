namespace Service.Models.Location;

public class LocationCreateModel
{
    public string Name { get; set; } = null!;

    public string Address { get; set; } = null!;
    
    public string Country { get; set; } = null!;

    public string City { get; set; } = null!;

    public long Longitude { get; set; }

    public long Latitude { get; set; }

    public string? Description { get; set; }

    public ICollection<Guid> Tags { get; set; } = new List<Guid>();
}