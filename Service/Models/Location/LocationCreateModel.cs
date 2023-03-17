namespace Service.Models.Location;

public class LocationCreateModel
{
    public string Name { get; set; } = null!;

    public string Address { get; set; } = null!;

    public long Longitude { get; set; }

    public long Latitude { get; set; }

    public string? Description { get; set; }
}