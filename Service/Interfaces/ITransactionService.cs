using Service.Models.Transaction;
using Shared.ExternalServices.VnPay.Models;
using Shared.ResultExtensions;

namespace Service.Interfaces;

public interface ITransactionService
{
    Task<Result<TransactionViewModel>> CreateTransaction(Guid bookingId, string clientIp);

    Task<Result> HandleIpnResponse(VnPayResponseModel model);
}