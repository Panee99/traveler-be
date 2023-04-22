using Data.Enums;

namespace Service.Models.Tour;

public class TourFilterViewModel
{
    public string Destination = null!;
    public DateTime EndTime;
    public Guid Id;
    public double Price;
    public DateTime StartTime;
    public string? ThumbnailUrl;
    public string Title = null!;
    public TourType Type;
    public string? Description;
}