
namespace Data.Entities;

public class VnPayResponse
{
    public string TmnCode { get; set; } = null!;
    public int Amount { get; set; }
    public string BankCode { get; set; } = null!;
    public string OrderInfo { get; set; } = null!;
    public string TransactionNo { get; set; } = null!;
    public string ResponseCode { get; set; } = null!;
    public string TransactionStatus { get; set; } = null!;
    public string SecureHash { get; set; } = null!;
    public DateTime Timestamp { get; set; }
    public string? BankTranNo { get; set; }
    public string? CardType { get; set; }
    public string? PayDate { get; set; }
    public string? SecureHashType { get; set; }
    
    /// <summary>
    /// Reference and Key
    /// </summary>
    public Guid TxnRef { get; set; }
    public virtual VnPayRequest? VnPayRequest { get; set; }
}