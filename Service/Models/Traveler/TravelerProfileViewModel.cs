using Data.Enums;

namespace Service.Models.Traveler;

public record TravelerProfileViewModel
{
    public Gender Gender;
    public string Phone = null!;
    public string FirstName = null!;
    public string LastName = null!;
    public string? BankName;
    public string? BankAccountNumber;
    public string? Address;
    public DateTime? BirthDay;
}