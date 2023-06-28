using Service.Models.User;

namespace Service.Models.TourGuide;

public class TourGuideViewModel : UserViewModel
{
    public string? FirstContactNumber { get; set; } = null!;
    public string? SecondContactNumber { get; set; } = null!;
}