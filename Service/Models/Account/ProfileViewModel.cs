using Data.Enums;

namespace Service.Models.Account;

public class ProfileViewModel
{
    public string? Address;
    public DateTime? Birthday;
    public string? Email;
    public string FirstName = null!;
    public Gender Gender;
    public Guid Id;
    public string LastName = null!;
    public string Phone = null!;
}