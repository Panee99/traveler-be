namespace Service.Models.TourGroup;

public record TourGroupUpdateModel
{
    public string? GroupName;
    public Guid? TourGuideId;
    public int? MaxOccupancy;
}