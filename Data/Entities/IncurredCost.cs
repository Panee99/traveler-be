namespace Data.Entities;

public class IncurredCost
{
    public Guid Id { get; set; }

    public Guid ActivityId { get; set; }

    public double Amount { get; set; }

    public string Title { get; set; } = null!;

    public Guid? ImageId { get; set; }

    public string? Description { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Activity Activity { get; set; } = null!;

    public Attachment? Image { get; set; } = null!;
}