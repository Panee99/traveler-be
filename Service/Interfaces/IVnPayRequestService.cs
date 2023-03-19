using Shared.ExternalServices.VnPay.Models;
using Shared.ResultExtensions;

namespace Service.Interfaces;

public interface IVnPayRequestService
{
    Task<Result> Add(VnPayRequestModel model);
}