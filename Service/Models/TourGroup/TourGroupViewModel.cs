namespace Service.Models.TourGroup;

public class TourGroupViewModel
{
    public Guid Id;
    public Guid TourId;
    public DateTime CreatedAt;
    public string GroupName = null!;
    public Guid? TourGuideId;
    public int MaxOccupancy;
}