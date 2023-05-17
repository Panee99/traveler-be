using Data.Enums;
using Service.Models.User;

namespace Service.Models.Traveler;

public class TravelerViewModel : UserViewModel
{
    public string FirstName = null!;
    public string LastName = null!;
    public DateTime? BirthDay;
    public Gender Gender;
    public string? Address;
}