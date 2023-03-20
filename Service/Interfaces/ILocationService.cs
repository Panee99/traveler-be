using Service.Models.Attachment;
using Service.Models.Location;
using Service.Pagination;
using Shared.ResultExtensions;

namespace Service.Interfaces;

public interface ILocationService
{
    Task<Result<LocationViewModel>> Create(LocationCreateModel model);

    Task<Result<LocationViewModel>> Update(Guid id, LocationUpdateModel model);

    Task<Result> Delete(Guid id);

    Task<Result<LocationViewModel>> Find(Guid id);

    Task<Result<AttachmentViewModel>> CreateAttachment(Guid locationId, AttachmentCreateModel model);

    Task<Result> DeleteAttachment(Guid locationId, Guid attachmentId);

    Task<Result<List<AttachmentViewModel>>> ListAttachments(Guid id);

    Task<Result<PaginationModel<LocationViewModel>>> Filter(LocationFilterModel model);
}