using Service.Models.TourGroup;
using Service.Models.TourVariant;
using Shared.ResultExtensions;

namespace Service.Interfaces;

public interface ITourVariantService
{
    Task<Result<TourVariantViewModel>> Create(TourVariantCreateModel model);

    Task<Result<TourVariantViewModel>> Update(Guid id, TourVariantUpdateModel model);

    Task<Result> Delete(Guid id);

    Task<Result<List<TourGroupViewModel>>> ListGroupsByTourVariant(Guid tourVariantId);
}