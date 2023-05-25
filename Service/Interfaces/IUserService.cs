using Service.Commons.QueryExtensions;
using Service.Models.User;
using Shared.ResultExtensions;

namespace Service.Interfaces;

public interface IUserService
{
    Task<Result<UserViewModel>> Create(UserCreateModel model);

    Task<Result<UserViewModel>> Update(Guid id, UserUpdateModel model);
    
    Task<Result<PaginationModel<UserViewModel>>> Filter(UserFilterModel model);
    
    Task<Result<UserViewModel>> GetProfile(Guid id);
    
    Task<Result<UserViewModel>> UpdateProfile(Guid id, ProfileUpdateModel model);
    
    Task<Result<UserViewModel>> AdminGetUserById(Guid id);
    
    Task<Result> AdminDeleteUserById(Guid id);
}