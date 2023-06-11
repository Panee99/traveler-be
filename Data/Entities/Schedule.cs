namespace Data.Entities;

public class Schedule
{
    public Guid Id { get; set; }

    public Guid TourId { get; set; }

    public virtual Tour Tour { get; set; } = null!;

    public string Description { get; set; } = null!;

    public int Sequence { get; set; }
}