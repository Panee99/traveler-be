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
    public string? Guide;
    public string? ThumbnailUrl;
    public DateTime CreatedAt;
    public TourType Type;
}