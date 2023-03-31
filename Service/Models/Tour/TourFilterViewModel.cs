using Data.Enums;

namespace Service.Models.Tour;

public class TourFilterViewModel
{
    public Guid Id;
    public string Title = null!;
    public double Price;
    public string Destination = null!;
    public DateTime StartTime;
    public DateTime EndTime;
    public TourType Type;
    public string? ThumbnailUrl;
}