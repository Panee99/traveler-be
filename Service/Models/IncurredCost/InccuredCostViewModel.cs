namespace Service.Models.IncurredCost;

public class IncurredCostViewModel
{
    public Guid Id { get; set; }

    public Guid TourGroupId { get; set; }

    public string Title { get; set; } = null!;

    public double Amount { get; set; }

    public DateTime CreatedAt { get; set; }

    public string? Description { get; set; }

    public string? ImageUrl { get; set; }
}