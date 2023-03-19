using Shared.ExternalServices.VnPay.Models;
using Shared.ResultExtensions;

namespace Service.Interfaces;

public interface IVnPayResponseService
{
    Task<Result> Add(VnPayResponseModel model);
}