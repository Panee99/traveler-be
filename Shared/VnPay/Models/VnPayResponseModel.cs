namespace Shared.VnPay.Models;

public class VnPayResponseModel
{
    public int Amount { get; set; }

    public string BankCode { get; set; }

    public string BankTranNo { get; set; }

    public string CardType { get; set; }
}