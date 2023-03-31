namespace Shared.ExternalServices.VnPay;

public class OrderInfo
{
    public long OrderId { get; set; }
    public int Amount { get; set; }
    public string Status { get; set; } = null!;
    public string OrderDesc { get; set; } = null!;
    public DateTime CreatedDate { get; set; }
}