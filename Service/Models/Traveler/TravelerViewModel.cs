using Data.Enums;
using Service.Models.Account;

namespace Service.Models.Traveler;

public class TravelerViewModel : AccountViewModel
{
    public string FirstName = null!;
    public string LastName = null!;
    public DateTime? BirthDay;
    public Gender Gender;
    public string? Address;
}