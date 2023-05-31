using Service.Models.Tour;
using Service.Models.TourGroup;
using Service.Models.TourGuide;
using Shared.ResultExtensions;

namespace Service.Interfaces;

public interface ITourGuideService
{
    Task<Result<List<TourFilterViewModel>>> ListAssignedTours(Guid tourGuideId);
    Task<Result<List<TourGroupViewModel>>> ListAssignedGroups(Guid tourGuideId);
}