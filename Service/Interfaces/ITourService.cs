using Service.Models.Attachment;
using Service.Models.Tour;
using Shared.ResultExtensions;

namespace Service.Interfaces;

public interface ITourService
{
    Task<Result<TourViewModel>> Create(TourCreateModel model);

    Task<Result<TourViewModel>> Update(Guid id, TourUpdateModel model);

    Task<Result> Delete(Guid id);

    Task<Result<TourViewModel>> Find(Guid id);

    Task<Result<AttachmentViewModel>> UpdateThumbnail(Guid id, string contentType, Stream stream);
}