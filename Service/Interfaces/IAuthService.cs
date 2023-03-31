using Service.Models.Auth;
using Shared.ResultExtensions;

namespace Service.Interfaces;

public interface IAuthService
{
    Task<Result<AuthenticateResponseModel>> AuthenticateTraveler(PhoneLoginModel model);
    Task<Result<AuthenticateResponseModel>> AuthenticateManager(EmailLoginModel model);
    Task<Result<AuthenticateResponseModel>> AuthenticateTourGuide(EmailLoginModel model);
}