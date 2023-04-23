using Service.Models.InccuredCost;
using Shared.ResultExtensions;

namespace Service.Interfaces;

public interface IIncurredCostService
{
    Task<Result<InccuredCostViewModel>> Create(IncurredCostCreateModel model);

    Task<Result> Delete(Guid incurredCostId);

    Task<Result<List<InccuredCostViewModel>>> List(Guid tourId, Guid tourGuideId);
}