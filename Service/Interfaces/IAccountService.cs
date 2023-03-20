using Service.Models.Account;
using Service.Models.Attachment;
using Shared.Enums;
using Shared.ResultExtensions;

namespace Service.Interfaces;

public interface IAccountService
{
    Task<Result<AttachmentViewModel>> UpdateAvatar(Guid id, string contentType, Stream stream);

    Task<Result<ProfileViewModel>> GetProfile(Guid id, UserRole role);

    Task<Result<AvatarViewModel>> GetAvatar(Guid id);
}