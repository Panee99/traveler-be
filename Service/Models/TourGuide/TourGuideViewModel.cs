using Data.Enums;
using Service.Models.User;

namespace Service.Models.TourGuide;

public class TourGuideViewModel : UserViewModel
{
    public DateTime? Birthday { get; set; }
}