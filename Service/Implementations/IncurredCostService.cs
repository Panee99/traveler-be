using Data.EFCore;
using Data.Entities;
using Mapster;
using Microsoft.EntityFrameworkCore;
using Service.Interfaces;
using Service.Models.IncurredCost;
using Shared.Helpers;
using Shared.ResultExtensions;

namespace Service.Implementations;

public class IncurredCostService : BaseService, IIncurredCostService
{
    private readonly ICloudStorageService _cloudStorageService;

    public IncurredCostService(UnitOfWork unitOfWork,
        ICloudStorageService cloudStorageService) : base(unitOfWork)
    {
        _cloudStorageService = cloudStorageService;
    }

    public async Task<Result<IncurredCostViewModel>> Create(IncurredCostCreateModel model)
    {
        var incurredCost = model.Adapt<IncurredCost>();
        incurredCost.CreatedAt = DateTimeHelper.VnNow();

        UnitOfWork.IncurredCosts.Add(incurredCost);
        await UnitOfWork.SaveChangesAsync();

        var view = incurredCost.Adapt<IncurredCostViewModel>();
        view.ImageUrl = _cloudStorageService.GetMediaLink(model.ImageId);

        return view;
    }

    public async Task<Result> Delete(Guid incurredCostId)
    {
        var incurredCost = await UnitOfWork.IncurredCosts.FindAsync(incurredCostId);
        if (incurredCost is null) return Error.NotFound();

        UnitOfWork.IncurredCosts.Remove(incurredCost);
        await UnitOfWork.SaveChangesAsync();

        return Result.Success();
    }

    public async Task<Result<List<IncurredCostViewModel>>> ListAll(Guid tourGroupId)
    {
        var incurredCosts = await UnitOfWork.IncurredCosts
            .Query()
            .Where(e => e.TourGroupId == tourGroupId)
            .ToListAsync();

        return incurredCosts.Select(e =>
        {
            var view = e.Adapt<IncurredCostViewModel>();
            view.ImageUrl = _cloudStorageService.GetMediaLink(e.ImageId);
            return view;
        }).ToList();
    }
}