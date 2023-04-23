namespace Service.Models.InccuredCost;

public class InccuredCostViewModel
{
    public Guid Id { get; set; }

    public Guid TourId { get; set; }

    public double Amount { get; set; }

    public DateTime CreatedAt { get; set; }

    public string? Description { get; set; }

    public Guid TourGuideId { get; set; }
}