namespace Shared.ExternalServices.VnPay.Models;

public class VnPayRequestModel
{
    public string Command { get; set; } = null!;
    public string TmnCode { get; set; } = null!;
    public int Amount { get; set; }
    public DateTime CreateDate { get; set; }
    public DateTime ExpireDate { get; set; }
    public string IpAddress { get; set; } = null!;
    public string OrderInfo { get; set; } = null!;
    public string ReturnUrl { get; set; } = null!;
    public Guid TxnRef { get; set; }
    public string Version { get; set; } = null!;
    public string CurrencyCode { get; set; } = null!;

    public string Locale { get; set; } = null!;

    public string? OrderType { get; set; }
    public string? BankCode { get; set; }

    public Guid TransactionId { get; set; }
}