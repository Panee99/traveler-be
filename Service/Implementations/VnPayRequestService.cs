using Data;
using Data.Entities;
using MapsterMapper;
using Service.Interfaces;
using Service.Results;
using Shared.VnPay.Models;

namespace Service.Implementations;

public class VnPayRequestService : IVnPayRequestService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public VnPayRequestService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result> Add(VnPayRequestModel model)
    {
        var entity = _mapper.Map<VnPayRequest>(model);
        _unitOfWork.Repo<VnPayRequest>().Add(entity);
        await _unitOfWork.SaveChangesAsync();
        return Result.Success();
    }
}