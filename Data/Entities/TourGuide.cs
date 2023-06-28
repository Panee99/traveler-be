namespace Data.Entities;

public class TourGuide : User
{
    public string? FirstContactNumber { get; set; }
    public string? SecondContactNumber { get; set; }
    public virtual ICollection<TourGroup> TourGroups { get; set; } = new List<TourGroup>();
}