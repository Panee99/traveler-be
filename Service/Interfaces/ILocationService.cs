using Service.Models.Attachment;
using Service.Models.Location;
using Service.Results;

namespace Service.Interfaces;

public interface ILocationService
{
    Task<Result<LocationViewModel>> Create(LocationCreateModel model);

    Task<Result<LocationViewModel>> Update(Guid id, LocationUpdateModel model);

    Task<Result> Delete(Guid id);

    Task<Result<LocationViewModel>> Find(Guid id);

    Task<Result> CreateAttachment(Guid locationId, AttachmentCreateModel model);
}