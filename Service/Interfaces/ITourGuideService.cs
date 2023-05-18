using Service.Models.Tour;
using Service.Models.TourGuide;
using Shared.ResultExtensions;

namespace Service.Interfaces;

public interface ITourGuideService
{
    Task<Result<List<TourFilterViewModel>>> ListAssignedTours(Guid tourGuideId);
}