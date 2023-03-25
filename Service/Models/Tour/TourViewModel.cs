using Data.Enums;

namespace Service.Models.Tour;

public record TourViewModel
{
    public Guid Id;
    public string Title = null!;
    public string Code = null!;
    public double Price;
    public double ChildrenPrice;
    public double BabyPrice;
    public string Departure = null!;
    public string Destination = null!;
    public DateTime StartTime;
    public DateTime EndTime;
    public string Vehicle = null!;
    public int MaxOccupancy;
    public TourType Type;
    public string? Description;
    public string? ThumbnailUrl;
}