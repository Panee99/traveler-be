using Data.Enums;

namespace Data.Entities;

public class Account
{
    public Guid Id { get; set; }

    public string Phone { get; set; } = null!;

    public string? Email { get; set; } = null!;

    public string Password { get; set; } = null!;

    public string? BankName { get; set; }

    public string? BankAccountNumber { get; set; }

    public AccountRole Role { get; set; }

    public AccountStatus Status { get; set; }

    public Guid? AvatarId { get; set; }

    public Attachment? Avatar { get; set; }

    // public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
}