using Data.Enums;

namespace Data.Entities;

public class TourGuide : Account
{
    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public Gender Gender { get; set; }

    public DateTime? Birthday { get; set; }

    public virtual ICollection<TourGroup> TourGroups { get; set; } = new List<TourGroup>();

    // public virtual ICollection<IncurredCost> IncurredCosts { get; set; } = new List<IncurredCost>();
}