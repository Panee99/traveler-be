namespace Service.Models.TourGroup;

public class TourGroupViewModel
{
    public Guid Id;
    public Guid? TourGuideId;
    public string GroupName = null!;
    public DateTime CreatedAt;
}