namespace Data.Entities;

public class VnPayRequest
{
    /// <summary>
    /// Key
    /// </summary>
    public Guid TxnRef { get; set; }
    public string Version { get; set; } = null!;
    public string Command { get; set; } = null!;
    public string TmnCode { get; set; } = null!;
    public int Amount { get; set; }
    public DateTime CreateDate { get; set; }
    public DateTime ExpireDate { get; set; }
    public string CurrencyCode { get; set; } = null!;
    public string IpAddress { get; set; } = null!;
    public string OrderInfo { get; set; } = null!;
    public string ReturnUrl { get; set; } = null!;
    public string Locale { get; set; } = null!;

    public string? OrderType { get; set; }
    public string? BankCode { get; set; }

    /// <summary>
    /// Reference
    /// </summary>
    public virtual VnPayResponse? VnPayResponse { get; set; }
}