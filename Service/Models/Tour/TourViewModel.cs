using Data.Enums;

namespace Service.Models.Tour;

public record TourViewModel
{
    public Guid Id;
    public string Title = null!;
    public double BabyPrice;
    public double ChildrenPrice;
    public string Code = null!;
    public string Departure = null!;
    public string Destination = null!;
    public DateTime EndTime;
    public int MaxOccupancy;
    public double Price;
    public DateTime StartTime;
    public string? Description;
    public string? ThumbnailUrl;
    public TourType Type;
    public TourStatus Status;
}