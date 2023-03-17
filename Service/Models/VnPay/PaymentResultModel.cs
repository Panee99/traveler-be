namespace Service.Models.VnPay;

public class PaymentResultModel
{
    public string OrderInfo { get; set; } = "";
    public int Amount { get; set; }
    public string BankCode { get; set; } = "";
    public string? CardType { get; set; } = "";
    public string Response { get; set; } = "";
    public string TransactionStatus { get; set; } = "";
    public string TransactionNo { get; set; } = "";
    public DateTime? PayDate { get; set; }
}