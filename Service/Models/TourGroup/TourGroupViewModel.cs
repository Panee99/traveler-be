namespace Service.Models.TourGroup;

public class TourGroupViewModel
{
    public DateTime CreatedAt;
    public string GroupName = null!;
    public Guid Id;
    public Guid? TourGuideId;
}