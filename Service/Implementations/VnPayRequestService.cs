using Data.EFCore;
using Data.Entities;
using MapsterMapper;
using Service.Interfaces;
using Shared.ExternalServices.VnPay.Models;
using Shared.ResultExtensions;

namespace Service.Implementations;

public class VnPayRequestService : BaseService, IVnPayRequestService
{
    private readonly IMapper _mapper;

    public VnPayRequestService(IUnitOfWork unitOfWork, IMapper mapper) : base(unitOfWork)
    {
        _mapper = mapper;
    }

    public async Task<Result> Add(VnPayRequestModel model)
    {
        var entity = _mapper.Map<VnPayRequest>(model);
        UnitOfWork.Repo<VnPayRequest>().Add(entity);
        await UnitOfWork.SaveChangesAsync();
        return Result.Success();
    }
}