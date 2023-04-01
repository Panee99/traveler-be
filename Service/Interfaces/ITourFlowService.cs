using Service.Models.TourFlow;
using Shared.ResultExtensions;

namespace Service.Interfaces;

public interface ITourFlowService
{
    Task<Result<TourFlowViewModel>> Create(TourFlowCreateModel model);

    Task<Result<TourFlowViewModel>> Update(Guid tourId, Guid locationId, TourFlowUpdateModel model);

    Task<Result> Delete(Guid tourId, Guid locationId);

    Task<Result<List<TourFlowViewModel>>> ListByTour(Guid tourId);
}