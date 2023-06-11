using Data.Enums;

namespace Data.Entities;

public class Transaction
{
    public Guid Id { get; set; }

    public double Amount { get; set; }

    public TransactionStatus Status { get; set; }

    public string ClientIp { get; set; } = null!;

    public DateTime Timestamp { get; set; }

    /// <summary>
    /// Transaction of which Booking
    /// </summary>
    public Guid BookingId { get; set; }

    public virtual Booking Booking { get; set; } = null!;
}