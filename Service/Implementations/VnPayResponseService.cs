using Data;
using Data.Entities;
using MapsterMapper;
using Microsoft.Extensions.Logging;
using Service.Interfaces;
using Service.Results;
using Shared;
using Shared.VnPay.Models;

namespace Service.Implementations;

public class VnPayResponseService : BaseService, IVnPayResponseService
{
    private readonly IMapper _mapper;
    private readonly ILogger<VnPayResponseService> _logger;

    public VnPayResponseService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<VnPayResponseService> logger) : base(unitOfWork)
    {
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result> Add(VnPayResponseModel model)
    {
        if (!_unitOfWork.Repo<VnPayRequest>().Any(e => e.TxnRef == model.TxnRef))
        {
            _logger.LogError("Invalid response. Can not find VnPay TxnRef: {TxnRef}", model.TxnRef);
            return Error.Unexpected();
        }
        
        var entity = _mapper.Map<VnPayResponse>(model);
        entity.Timestamp = DateTimeHelper.VnNow();
        
        _unitOfWork.Repo<VnPayResponse>().Add(entity);
        await _unitOfWork.SaveChangesAsync();
        return Result.Success();
    }
}