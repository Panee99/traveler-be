using Data.EFCore;
using Data.Entities;
using Data.Enums;
using Mapster;
using Microsoft.EntityFrameworkCore;
using Service.Commons;
using Service.Commons.Mapping;
using Service.Interfaces;
using Service.Models.TourGroup;
using Service.Models.TourVariant;
using Shared.ResultExtensions;

namespace Service.Implementations;

public class TourVariantService : BaseService, ITourVariantService
{
    public TourVariantService(UnitOfWork unitOfWork) : base(unitOfWork)
    {
    }

    public async Task<Result<TourVariantViewModel>> Create(TourVariantCreateModel model)
    {
        var tourVariant = model.Adapt<TourVariant>();
        tourVariant.Code = CodeGenerator.NewCode();
        tourVariant.Status = TourVariantStatus.Accepting;

        UnitOfWork.TourVariants.Add(tourVariant);
        await UnitOfWork.SaveChangesAsync();

        return tourVariant.Adapt<TourVariantViewModel>();
    }

    public async Task<Result<TourVariantViewModel>> Update(Guid id, TourVariantUpdateModel model)
    {
        var tourVariant = await UnitOfWork.TourVariants.FindAsync(id);
        if (tourVariant is null) return Error.NotFound();

        model.AdaptIgnoreNull(tourVariant);
        UnitOfWork.TourVariants.Update(tourVariant);

        await UnitOfWork.SaveChangesAsync();
        return tourVariant.Adapt<TourVariantViewModel>();
    }

    public async Task<Result<TourVariantViewModel>> Get(Guid id)
    {
        var tourVariant = await UnitOfWork.TourVariants.FindAsync(id);
        if (tourVariant is null) return Error.NotFound();
        return tourVariant.Adapt<TourVariantViewModel>();
    }

    public async Task<Result> Delete(Guid id)
    {
        var tourVariant = await UnitOfWork.TourVariants.FindAsync(id);
        if (tourVariant is null) return Error.NotFound();

        UnitOfWork.TourVariants.Remove(tourVariant);
        await UnitOfWork.SaveChangesAsync();

        return Result.Success();
    }
    
    public async Task<Result<List<TourGroupViewModel>>> ListGroupsByTourVariant(Guid tourVariantId)
    {
        if (!await UnitOfWork.TourVariants.AnyAsync(e => e.Id == tourVariantId)) return Error.NotFound();
        
        var tourGroups = await UnitOfWork.TourGroups
            .Query()
            .Where(e => e.TourVariantId == tourVariantId)
            .ToListAsync();
        
        return tourGroups.Adapt<List<TourGroupViewModel>>();
    }
}


