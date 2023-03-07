using Data.Models.Get;
using Data.Models.View;

namespace Service.Interfaces;

public interface IAuthService
{
    Task<TokenViewModel> AuthenticateManager(AuthRequestModel model);
    Task<ManagerViewModel> GetManagerById(Guid id);
    Task<TokenViewModel> AuthenticateTourGuide(AuthRequestModel model);
    Task<TourGuideViewModel> GetTourGuideById(Guid id);
    Task<TokenViewModel> AuthenticateTraveler(AuthRequestModel model);
    Task<TravelerViewModel> GetTravelerById(Guid id);
    Task<AuthViewModel> AuthById(Guid id);
}