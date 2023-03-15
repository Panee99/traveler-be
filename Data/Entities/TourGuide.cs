﻿using Data.Enums;

namespace Data.Entities;

public class TourGuide
{
    public Guid Id { get; set; }

    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public string? AvatarUrl { get; set; }

    public DateTime? Birthday { get; set; }

    public Gender Gender { get; set; }

    public Guid AccountId { get; set; }

    public virtual Account Account { get; set; } = null!;

    public virtual ICollection<IncurredCost> IncurredCosts { get; set; } = new List<IncurredCost>();
}