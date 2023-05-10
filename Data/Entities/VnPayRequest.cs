// using Data.Enums;
//
// namespace Data.Entities;
//
// public class VnPayRequest
// {
//     /// <summary>
//     /// Key
//     /// </summary>
//     public Guid TxnRef { get; set; }
//
//     public int Amount { get; set; }
//     public string Version { get; set; } = null!;
//     public string Command { get; set; } = null!;
//     public string TmnCode { get; set; } = null!;
//     public string CurrencyCode { get; set; } = null!;
//     public string IpAddress { get; set; } = null!;
//     public string OrderInfo { get; set; } = null!;
//     public string ReturnUrl { get; set; } = null!;
//     public string Locale { get; set; } = null!;
//     public string? OrderType { get; set; }
//     public string? BankCode { get; set; }
//     public DateTime CreateDate { get; set; }
//     public DateTime ExpireDate { get; set; }
//     public VnPayRequestStatus Status { get; set; }
//
//     /// <summary>
//     /// Pay request for a Transaction
//     /// </summary>
//     public Guid TransactionId { get; set; }
//
//     public virtual Transaction Transaction { get; set; } = null!;
// }