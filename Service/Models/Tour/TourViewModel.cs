using Data.Enums;

namespace Service.Models.Tour;

public record TourViewModel
{
    public double BabyPrice;
    public double ChildrenPrice;
    public string Code = null!;
    public string Departure = null!;
    public string? Description;
    public string Destination = null!;
    public DateTime EndTime;
    public Guid Id;
    public int MaxOccupancy;
    public double Price;
    public DateTime StartTime;
    public string? ThumbnailUrl;
    public string Title = null!;
    public TourType Type;
    public string Vehicle = null!;
    public TourStatus Status;
}