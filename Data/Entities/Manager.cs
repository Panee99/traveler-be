using Data.Enums;

namespace Data.Entities;

public class Manager : Account
{
    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public DateTime? Birthday { get; set; }

    public Gender Gender { get; set; }
}