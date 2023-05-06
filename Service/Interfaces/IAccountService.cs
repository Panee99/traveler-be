using Data.Enums;
using Service.Models.Account;
using Shared.ResultExtensions;

namespace Service.Interfaces;

public interface IAccountService
{
    Task<Result<AccountViewModel>> GetProfile(Guid id, AccountRole role);

    Task<Result<AccountViewModel>> UpdateProfile(Guid id, ProfileUpdateModel model);
}