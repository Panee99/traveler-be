using Data.EFCore;
using Data.EFCore.Repositories;
using Data.Entities;
using Mapster;
using Microsoft.EntityFrameworkCore;
using Service.Interfaces;
using Service.Models.InccuredCost;
using Shared.ResultExtensions;

namespace Service.Implementations;

public class IncurredCostService : BaseService, IIncurredCostService
{
    private readonly IRepository<IncurredCost> _incurredCostRepo;

    public IncurredCostService(IUnitOfWork unitOfWork) : base(unitOfWork)
    {
        _incurredCostRepo = unitOfWork.Repo<IncurredCost>();
    }

    public async Task<Result<InccuredCostViewModel>> Create(IncurredCostCreateModel model)
    {
        var incurredCost = model.Adapt<IncurredCost>();

        _incurredCostRepo.Add(incurredCost);

        await UnitOfWork.SaveChangesAsync();

        return incurredCost.Adapt<InccuredCostViewModel>();
    }

    public async Task<Result> Delete(Guid incurredCostId)
    {
        var incurredCost = await _incurredCostRepo.FindAsync(incurredCostId);
        if (incurredCost is null) return Error.NotFound();

        _incurredCostRepo.Remove(incurredCost);

        await UnitOfWork.SaveChangesAsync();

        return Result.Success();
    }

    public async Task<Result<List<InccuredCostViewModel>>> List(Guid tourId, Guid tourGuideId)
    {
        var incurredCosts = await _incurredCostRepo
            .Query()
            .Where(e => e.TourId == tourId && e.TourGuideId == tourGuideId)
            .ToListAsync();

        return incurredCosts.Adapt<List<InccuredCostViewModel>>();
    }
}