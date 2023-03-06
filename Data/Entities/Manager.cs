using System;
using System.Collections.Generic;

namespace Data.Entities;

public partial class Manager
{
    public Guid Id { get; set; }

    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public DateTime? Birthday { get; set; }

    public string? AvatarUrl { get; set; }

    public string Gender { get; set; } = null!;

    public Guid AccountId { get; set; }

    public virtual Account Account { get; set; } = null!;
}
