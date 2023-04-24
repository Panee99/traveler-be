using Data.Enums;

namespace Service.Models.Tour;

public class TourFilterViewModel
{
    public Guid Id;
    public string Title = null!;
    public string Departure = null!;
    public string Destination = null!;
    public DateTime StartTime;
    public DateTime EndTime;
    public double AdultPrice;
    public string? ThumbnailUrl;
    public string? Description;
    public TourType Type;
}