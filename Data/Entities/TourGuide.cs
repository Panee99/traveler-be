using Data.Enums;

namespace Data.Entities;

public class TourGuide : User
{
    public DateTime? Birthday { get; set; }

    public virtual ICollection<TourGroup> TourGroups { get; set; } = new List<TourGroup>();

    // public virtual ICollection<IncurredCost> IncurredCosts { get; set; } = new List<IncurredCost>();
}