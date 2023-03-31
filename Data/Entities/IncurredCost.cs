namespace Data.Entities;

public class IncurredCost
{
    public Guid Id { get; set; }

    public Guid TourId { get; set; }

    public double Amount { get; set; }

    public DateTime CreatedAt { get; set; }

    public string? Description { get; set; }

    public Guid TourGuideId { get; set; }

    public virtual Tour Tour { get; set; } = null!;

    public virtual TourGuide TourGuide { get; set; } = null!;
}