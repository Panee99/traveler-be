using Service.Results;
using Shared.VnPay.Models;

namespace Service.Interfaces;

public interface IVnPayRequestService
{
    Task<Result> Add(VnPayRequestModel model);
}