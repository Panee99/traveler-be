using Service.Models.Account;
using Service.Models.Attachment;
using Service.Results;
using Shared.Enums;

namespace Service.Interfaces;

public interface IAccountService
{
    Task<Result<AttachmentViewModel>> UpdateAvatar(Guid id, string contentType, Stream stream);
    
    Result<ProfileViewModel> GetProfile(Guid id, UserRole role);

    Task<Result<AvatarViewModel>> GetAvatar(Guid id);
}