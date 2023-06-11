using Data.Enums;
using Service.Models.User;

namespace Service.Models.Traveler;

public class TravelerViewModel : UserViewModel
{
    public string? BankName;
    public string? BankNumber;
    public DateTime? BirthDay;
    public string? Address;
}