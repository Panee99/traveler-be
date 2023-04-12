using Service.Models.Auth;
using Shared.ResultExtensions;

namespace Service.Interfaces;

public interface IAuthService
{
    Task<Result<AuthenticateResponseModel>> Authenticate(LoginModel model);
}