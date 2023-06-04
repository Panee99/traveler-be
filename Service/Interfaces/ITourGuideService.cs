using Service.Models.Tour;
using Service.Models.TourGroup;
using Shared.ResultExtensions;

namespace Service.Interfaces;

public interface ITourGuideService
{
    Task<Result<List<TourViewModel>>> ListAssignedTours(Guid tourGuideId);

    Task<Result<List<TourGroupViewModel>>> ListAssignedGroups(Guid tourGuideId);

    Task<Result<TourGroupViewModel>> GetCurrentAssignedTourGroup(Guid tourGuideId);
}