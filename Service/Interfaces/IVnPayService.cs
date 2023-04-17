using Shared.ExternalServices.VnPay.Models;
using Shared.ResultExtensions;

namespace Service.Interfaces;

public interface IVnPayService
{
    /// <summary>
    /// Create new pay request
    /// </summary>
    /// <returns>TxnRef code, identify request</returns>
    Task<Result<Guid>> CreateRequest(VnPayRequestModel model);

    /// <summary>
    /// Handle pay response
    /// </summary>
    /// <returns>TxnRef code, identify request</returns>
    Task<Result> HandleResponse(VnPayResponseModel model);
}