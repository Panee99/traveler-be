namespace Service.Models.InccuredCost;

public class IncurredCostCreateModel
{
    public Guid TourId { get; set; }

    public Guid TourGuideId { get; set; }

    public double Amount { get; set; }

    public DateTime CreatedAt { get; set; }

    public string? Description { get; set; }
}