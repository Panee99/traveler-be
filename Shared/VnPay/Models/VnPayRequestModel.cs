namespace Shared.VnPay.Models;

public class VnPayRequestModel
{
    public string Command { get; set; } = "";
    public string TmnCode { get; set; } = "";
    public int Amount { get; set; }
    public DateTime CreateDate { get; set; }
    public DateTime ExpireDate { get; set; }
    public string IpAddress { get; set; } = "";
    public string OrderInfo { get; set; } = "";
    public string ReturnUrl { get; set; } = "";
    public string TxnRef { get; set; } = "";
}