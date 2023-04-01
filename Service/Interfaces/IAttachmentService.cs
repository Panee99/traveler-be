using Shared.ResultExtensions;

namespace Service.Interfaces;

public interface IAttachmentService
{
    Task<Result> Delete(Guid id);
}