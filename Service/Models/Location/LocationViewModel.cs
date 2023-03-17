using Data.Entities;
using Service.Models.Tag;

namespace Service.Models.Location;

public class LocationViewModel
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public string Address { get; set; } = null!;

    public double Longitude { get; set; }

    public double Latitude { get; set; }

    public ICollection<Attachment>? Attachments { get; set; } = null;

    public ICollection<TagViewModel>? Tags { get; set; } = null;
}