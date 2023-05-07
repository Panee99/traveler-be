namespace Data.Entities;

public class TourFlow
{
    public Guid Id { get; set; }

    public Guid TourId { get; set; }

    public virtual Tour Tour { get; set; } = null!;

    public string Description { get; set; } = null!;

    public DateTime? From { get; set; }

    public DateTime? To { get; set; }
    
    public int Sequence { get; set; }
}