using Service.Models.Account;
using Service.Models.Tour;
using Service.Models.Traveler;
using Shared.ResultExtensions;

namespace Service.Interfaces;

public interface ITravelerService
{
    Task<Result> Register(TravelerRegistrationModel model);

    Task<Result<TravelerProfileViewModel>> GetProfile(Guid id);

    Task<Result<List<ProfileViewModel>>> ListByTour(Guid tourId);

    Task<Result<List<TourFilterViewModel>>> ListJoinedTours(Guid travelerId);
}