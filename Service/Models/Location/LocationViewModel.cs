using Service.Models.Tag;

namespace Service.Models.Location;

public record LocationViewModel
{
    public string Address = null!;
    public string City = null!;
    public string Country = null!;
    public string? Description;
    public Guid Id;
    public double Latitude;
    public double Longitude;
    public string Name = null!;
    public ICollection<TagViewModel> Tags = new List<TagViewModel>();
}