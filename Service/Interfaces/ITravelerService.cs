using Service.Models.Tour;
using Service.Models.Traveler;
using Shared.ResultExtensions;

namespace Service.Interfaces;

public interface ITravelerService
{
    Task<Result> Register(TravelerRegistrationModel model);

    Task<Result<List<TravelerViewModel>>> ListByTour(Guid tourId);

    Task<Result<List<TourFilterViewModel>>> ListJoinedTours(Guid travelerId);
}