using Service.Results;
using Shared.VnPay.Models;

namespace Service.Interfaces;

public interface IVnPayResponseService
{
    Task<Result> Add(VnPayResponseModel model);
}