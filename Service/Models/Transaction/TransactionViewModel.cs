using Data.Enums;

namespace Service.Models.Transaction;

public record TransactionViewModel
{
    public Guid Id;
    public Guid BookingId;
    public double Amount;
    public TransactionStatus Status;
    public DateTime Timestamp;
    public string PayUrl = null!;
}