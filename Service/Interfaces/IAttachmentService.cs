using Service.Models.Attachment;
using Shared.ResultExtensions;

namespace Service.Interfaces;

public interface IAttachmentService
{
    Task<Result<AttachmentViewModel>> Create(string contentType, Stream stream);

    Task<Result> Delete(Guid id);
}