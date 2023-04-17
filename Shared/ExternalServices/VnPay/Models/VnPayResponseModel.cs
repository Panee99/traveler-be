namespace Shared.ExternalServices.VnPay.Models;

public class VnPayResponseModel
{
    public Guid TxnRef { get; set; }
    public string TmnCode { get; set; } = null!;
    public long Amount { get; set; }
    public string BankCode { get; set; } = null!;
    public string OrderInfo { get; set; } = null!;
    public string TransactionNo { get; set; } = null!;
    public string ResponseCode { get; set; } = null!;
    public string TransactionStatus { get; set; } = null!;
    public string SecureHash { get; set; } = null!;
    public string? BankTranNo { get; set; }
    public string? CardType { get; set; }
    public string? PayDate { get; set; }
    public string? SecureHashType { get; set; }
}