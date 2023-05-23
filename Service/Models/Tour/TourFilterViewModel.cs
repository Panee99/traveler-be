using Data.Enums;

namespace Service.Models.Tour;

public class TourFilterViewModel
{
    public Guid Id;
    public int MaxOccupancy;
    public string Title = null!;
    public string Departure = null!;
    public string Destination = null!;
    public string? ThumbnailUrl;
    public string? Description;
    public TourType Type;
    public TourStatus Status;
}