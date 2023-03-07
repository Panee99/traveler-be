namespace Data.Entities;

public class Transaction
{
    public Guid Id { get; set; }

    public double Amount { get; set; }

    public Guid AccountId { get; set; }

    public Guid? BookingId { get; set; }

    public DateTime CreatedAt { get; set; }

    public string? Description { get; set; }

    public string Type { get; set; } = null!;

    public string Status { get; set; } = null!;

    public virtual Account Account { get; set; } = null!;

    public virtual Booking? Booking { get; set; }
}
