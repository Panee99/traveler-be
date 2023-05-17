using Data.Enums;
using Service.Models.User;

namespace Service.Models.TourGuide;

public class TourGuideViewModel : UserViewModel
{
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public Gender Gender { get; set; }
    public DateTime? Birthday { get; set; }
}