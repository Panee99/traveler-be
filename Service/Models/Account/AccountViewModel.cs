using Data.Enums;

namespace Service.Models.Account;

public class AccountViewModel
{
    public Guid Id;
    public string Phone = null!;
    public string? Email;
    public string? BankName;
    public string? BankAccountNumber;
    public AccountRole Role { get; set; }
    public AccountStatus Status { get; set; }
    public string? AvatarUrl { get; set; }
}