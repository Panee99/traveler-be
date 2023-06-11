namespace Shared.ExternalServices.VnPay.Models;

public class VnPayRequestModel
{
    public int Amount { get; set; }
    public DateTime CreateDate { get; set; }
    public DateTime ExpireDate { get; set; }
    public string IpAddress { get; set; } = null!;
    public string OrderInfo { get; set; } = null!;
    public Guid TxnRef { get; set; }
    public string? OrderType { get; set; }
    public string? BankCode { get; set; }
}