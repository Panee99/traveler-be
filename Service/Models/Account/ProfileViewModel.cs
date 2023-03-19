using Data.Enums;

namespace Service.Models.Account;

public class ProfileViewModel
{
    public Guid Id;
    public string Phone = null!;
    public string? Email;
    public string FirstName = null!;
    public string LastName = null!;
    public Gender Gender;
    public string? Address;
    public DateTime? Birthday;
}