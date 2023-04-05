using Data.Enums;
using Service.Models.Account;
using Service.Models.Attachment;
using Shared.ResultExtensions;

namespace Service.Interfaces;

public interface IAccountService
{
    Task<Result<AttachmentViewModel>> UpdateAvatar(Guid id, string contentType, Stream stream);

    Task<Result<ProfileViewModel>> GetProfile(Guid id, AccountRole role);

    Task<Result<AvatarViewModel>> GetAvatar(Guid id);
}