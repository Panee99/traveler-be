using Data.EFCore;
using Data.EFCore.Repositories;
using Data.Entities;
using MapsterMapper;
using Microsoft.Extensions.Logging;
using Service.Interfaces;
using Shared.ExternalServices.VnPay.Models;
using Shared.Helpers;
using Shared.ResultExtensions;

namespace Service.Implementations;

public class VnPayResponseService : BaseService, IVnPayResponseService
{
    private readonly ILogger<VnPayResponseService> _logger;
    private readonly IMapper _mapper;
    private readonly IRepository<VnPayRequest> _vnPayRequestRepo;
    private readonly IRepository<VnPayResponse> _vnPayResponseRepo;
    
    public VnPayResponseService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<VnPayResponseService> logger) :
        base(unitOfWork)
    {
        _mapper = mapper;
        _logger = logger;
        //
        _vnPayRequestRepo = unitOfWork.Repo<VnPayRequest>();
        _vnPayResponseRepo = unitOfWork.Repo<VnPayResponse>();
    }

    public async Task<Result> Add(VnPayResponseModel model)
    {
        if (!await _vnPayRequestRepo.AnyAsync(e => e.TxnRef == model.TxnRef))
        {
            _logger.LogError("Invalid response. Can not find VnPay TxnRef: {TxnRef}", model.TxnRef);
            return Error.Unexpected();
        }

        var entity = _mapper.Map<VnPayResponse>(model);
        entity.Timestamp = DateTimeHelper.VnNow();

        _vnPayResponseRepo.Add(entity);
        await UnitOfWork.SaveChangesAsync();
        return Result.Success();
    }
}