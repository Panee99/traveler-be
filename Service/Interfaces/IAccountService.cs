using Service.Models.Attachment;
using Service.Results;

namespace Service.Interfaces;

public interface IAccountService
{
    Task<Result<AttachmentViewModel>> UpdateAvatar(Guid id, string contentType, Stream stream);
}