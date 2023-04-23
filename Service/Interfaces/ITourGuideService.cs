using Service.Models.Account;
using Service.Models.Tour;
using Service.Models.TourGuide;
using Shared.ResultExtensions;

namespace Service.Interfaces;

public interface ITourGuideService
{
    Task<Result<ProfileViewModel>> Create(TourGuideCreateModel model);

    Task<Result<List<TourFilterViewModel>>> ListAssignedTours(Guid tourGuideId);
    
    Task<Result<List<ProfileViewModel>>> ListAll();
}