using Data.Enums;

namespace Data.Entities;

public class Manager
{
    public Guid Id { get; set; }

    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public DateTime? Birthday { get; set; }

    public string? AvatarUrl { get; set; }

    public Gender Gender { get; set; }

    public Guid AccountId { get; set; }

    public virtual Account Account { get; set; } = null!;
}
