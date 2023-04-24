using Data.Enums;
using Service.Models.Location;

namespace Service.Models.Tour;

public record TourViewModel
{
    public Guid Id;
    public string Title = null!;
    public double AdultPrice;
    public double ChildrenPrice;
    public double InfantPrice;
    public string Code = null!;
    public string Departure = null!;
    public string Destination = null!;
    public DateTime EndTime;
    public int MaxOccupancy;
    public DateTime StartTime;
    public string? Description;
    public string? ThumbnailUrl;
    public TourType Type;
    public TourStatus Status;
    public List<LocationViewModel> TourFlow = new();
}