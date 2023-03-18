using Service.Models.Tag;

namespace Service.Models.Location;

public record LocationViewModel
{
    public Guid Id;
    public string Name = null!;
    public string Address = null!;
    public string Country = null!;
    public string City = null!;
    public double Longitude;
    public double Latitude;
    public string? Description;
    public ICollection<TagViewModel> Tags = new List<TagViewModel>();
}