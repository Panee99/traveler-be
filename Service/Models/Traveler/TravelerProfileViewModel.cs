using Data.Enums;

namespace Service.Models.Traveler;

public record TravelerProfileViewModel
{
    public string? Address;
    public string? BankAccountNumber;
    public string? BankName;
    public DateTime? BirthDay;
    public string FirstName = null!;
    public Gender Gender;
    public string LastName = null!;
    public string Phone = null!;
}