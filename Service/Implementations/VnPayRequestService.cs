using Data.EFCore;
using Data.EFCore.Repositories;
using Data.Entities;
using MapsterMapper;
using Service.Interfaces;
using Shared.ExternalServices.VnPay.Models;
using Shared.ResultExtensions;

namespace Service.Implementations;

public class VnPayRequestService : BaseService, IVnPayRequestService
{
    private readonly IMapper _mapper;
    private readonly IRepository<VnPayRequest> _vnPayRequestRepo;

    public VnPayRequestService(IUnitOfWork unitOfWork, IMapper mapper) : base(unitOfWork)
    {
        _mapper = mapper;
        _vnPayRequestRepo = unitOfWork.Repo<VnPayRequest>();
    }

    public async Task<Result> Add(VnPayRequestModel model)
    {
        var entity = _mapper.Map<VnPayRequest>(model);
        _vnPayRequestRepo.Add(entity);
        await UnitOfWork.SaveChangesAsync();
        return Result.Success();
    }
}