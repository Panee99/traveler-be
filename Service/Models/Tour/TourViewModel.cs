using Data.Enums;

namespace Service.Models.Tour;

public record TourViewModel
{
    public Guid Id;
    public string Title = null!;
    public string Departure = null!;
    public string Destination = null!;
    public string Duration = null!;
    public string? Description;
    public string? Policy;
    public string? ThumbnailUrl;
    public int MaxOccupancy;
    public TourType Type;
    public TourStatus Status;
}