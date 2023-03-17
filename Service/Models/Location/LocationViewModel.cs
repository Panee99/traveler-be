using Data.Entities;
using Service.Models.Tag;

namespace Service.Models.Location;

public class LocationViewModel
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public string Address { get; set; } = null!;

    public string Country { get; set; } = null!;

    public string City { get; set; } = null!;

    public double Longitude { get; set; }

    public double Latitude { get; set; }

    public string? Description { get; set; }

    public ICollection<string> Attachments { get; set; } = new List<string>();

    public ICollection<TagViewModel> Tags { get; set; } = new List<TagViewModel>();
}