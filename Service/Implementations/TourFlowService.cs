using Data.EFCore;
using Data.Entities;
using Mapster;
using Microsoft.EntityFrameworkCore;
using Service.Interfaces;
using Service.Models.TourFlow;
using Shared.Helpers;
using Shared.ResultExtensions;

namespace Service.Implementations;

public class TourFlowService : BaseService, ITourFlowService
{
    protected TourFlowService(IUnitOfWork unitOfWork) : base(unitOfWork)
    {
    }

    public async Task<Result<TourFlowViewModel>> Create(TourFlowCreateModel model)
    {
        if (!await UnitOfWork.Repo<Tour>().AnyAsync(e => e.Id == model.TourId))
            return Error.NotFound("Tour not found");

        if (!await UnitOfWork.Repo<Location>().AnyAsync(e => e.Id == model.LocationId))
            return Error.NotFound("Location not found");

        var entity = UnitOfWork.Repo<TourFlow>().Add(model.Adapt<TourFlow>());

        await UnitOfWork.SaveChangesAsync();

        return entity.Adapt<TourFlowViewModel>();
    }

    public async Task<Result<TourFlowViewModel>> Update(Guid tourId, Guid locationId, TourFlowUpdateModel model)
    {
        var tourFlow = await UnitOfWork
            .Repo<TourFlow>()
            .FirstOrDefaultAsync(e => e.TourId == tourId && e.LocationId == locationId);

        if (tourFlow == null) return Error.NotFound();

        UnitOfWork.Repo<TourFlow>()
            .Update(
                model.Adapt(tourFlow,
                    MapperHelper.IgnoreNullConfig<TourFlowUpdateModel, TourFlow>()));

        await UnitOfWork.SaveChangesAsync();

        return tourFlow.Adapt<TourFlowViewModel>();
    }


    public async Task<Result> Delete(Guid tourId, Guid locationId)
    {
        var isExist = await UnitOfWork
            .Repo<TourFlow>()
            .AnyAsync(e => e.TourId == tourId && e.LocationId == locationId);

        if (!isExist) return Error.NotFound();

        UnitOfWork.Repo<TourFlow>().Remove(new TourFlow()
        {
            TourId = tourId,
            LocationId = locationId
        });

        await UnitOfWork.SaveChangesAsync();

        return Result.Success();
    }

    public async Task<Result<List<TourFlowViewModel>>> ListByTour(Guid tourId)
    {
        var tourFlows = await UnitOfWork.Repo<TourFlow>()
            .Query()
            .Where(e => e.TourId == tourId)
            .ToListAsync();

        return tourFlows.Adapt<List<TourFlowViewModel>>();
    }
}