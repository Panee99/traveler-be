namespace Service.Models.IncurredCost;

public class IncurredCostCreateModel
{
    public Guid ActivityId { get; set; }

    public string Title { get; set; } = null!;

    public double Amount { get; set; }

    public string? Description { get; set; }

    public Guid? ImageId { get; set; }
}