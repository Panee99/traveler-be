using Data.Enums;

namespace Data.Entities;

public abstract class Account
{
    public Guid Id { get; set; }

    public string Phone { get; set; } = null!;

    public string Password { get; set; } = null!;

    public string? BankName { get; set; }

    public string? BankAccountNumber { get; set; }

    public AccountStatus Status { get; set; }

    public Guid? AttachmentId { get; set; }

    public Attachment? Attachment { get; set; }

    // public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
}