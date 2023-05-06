using Data.Enums;
using Service.Models.Account;

namespace Service.Models.TourGuide;

public class TourGuideViewModel : AccountViewModel
{
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public Gender Gender { get; set; }
    public DateTime? Birthday { get; set; }
}