using Service.Models.Transaction;
using Shared.ResultExtensions;

namespace Service.Interfaces;

public interface ITransactionService
{
    Task<Result<TransactionViewModel>> CreateTransaction(Guid bookingId, string clientIp);
}