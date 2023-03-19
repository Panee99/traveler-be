using Service.Models.Account;
using Service.Models.Auth;
using Shared.ResultExtensions;

namespace Service.Interfaces;

public interface IAuthService
{
    Result<AuthenticateResponseModel> AuthenticateTraveler(PhoneLoginModel model);
    Result<AuthenticateResponseModel> AuthenticateManager(EmailLoginModel model);
    Result<AuthenticateResponseModel> AuthenticateTourGuide(EmailLoginModel model);
}