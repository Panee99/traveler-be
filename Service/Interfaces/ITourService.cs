using Service.Models.Attachment;
using Service.Models.Location;
using Service.Models.Tour;
using Service.Pagination;
using Shared.ResultExtensions;

namespace Service.Interfaces;

public interface ITourService
{
    Task<Result<TourViewModel>> Create(TourCreateModel model);

    Task<Result<TourViewModel>> Update(Guid id, TourUpdateModel model);

    Task<Result> Delete(Guid id);

    Task<Result<TourViewModel>> Find(Guid id);

    Task<Result<PaginationModel<TourViewModel>>> Filter(TourFilterModel model);

    // Locations
    Task<Result<LocationViewModel>> AddLocation(Guid tourId, LocationCreateModel model);

    Task<Result> DeleteLocation(Guid id);

    Task<Result<List<LocationViewModel>>> ListLocations(Guid tourId);

    // Attachments
    Task<Result<AttachmentViewModel>> UpdateThumbnail(Guid tourId, string contentType, Stream stream);

    Task<Result<AttachmentViewModel>> AddAttachment(Guid tourId, string contentType, Stream stream);

    Task<Result> DeleteAttachment(Guid tourId, Guid attachmentId);

    Task<Result<List<AttachmentViewModel>>> ListAttachments(Guid tourId);
}