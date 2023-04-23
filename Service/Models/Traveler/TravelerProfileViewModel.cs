using Data.Enums;

namespace Service.Models.Traveler;

public class TravelerProfileViewModel
{
    public Guid Id;
    public string? Address;
    public string? BankAccountNumber;
    public string? BankName;
    public DateTime? BirthDay;
    public string FirstName = null!;
    public string LastName = null!;
    public Gender Gender;
    public string Phone = null!;
}