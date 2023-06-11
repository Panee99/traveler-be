using Data.EFCore;
using Data.Entities;
using Mapster;
using Microsoft.EntityFrameworkCore;
using Service.Interfaces;
using Service.Models.InccuredCost;
using Shared.ResultExtensions;

namespace Service.Implementations;

public class IncurredCostService : BaseService, IIncurredCostService
{
    public IncurredCostService(UnitOfWork unitOfWork) : base(unitOfWork)
    {
    }

    public async Task<Result<InccuredCostViewModel>> Create(IncurredCostCreateModel model)
    {
        // var incurredCost = model.Adapt<IncurredCost>();
        //
        // UnitOfWork.IncurredCosts.Add(incurredCost);
        //
        // await UnitOfWork.SaveChangesAsync();
        //
        // return incurredCost.Adapt<InccuredCostViewModel>();

        throw new NotImplementedException();
    }

    public async Task<Result> Delete(Guid incurredCostId)
    {
        // var incurredCost = await UnitOfWork.IncurredCosts.FindAsync(incurredCostId);
        // if (incurredCost is null) return Error.NotFound();
        //
        // UnitOfWork.IncurredCosts.Remove(incurredCost);
        //
        // await UnitOfWork.SaveChangesAsync();
        //
        // return Result.Success();

        throw new NotImplementedException();
    }

    public async Task<Result<List<InccuredCostViewModel>>> List(Guid tourId, Guid tourGuideId)
    {
        // var incurredCosts = await UnitOfWork.IncurredCosts
        //     .Query()
        //     .Where(e => e.TourGroupId == tourId && e.TourGroup.TourGuideId == tourGuideId)
        //     .ToListAsync();
        //
        // return incurredCosts.Adapt<List<InccuredCostViewModel>>();

        throw new NotImplementedException();
    }
}