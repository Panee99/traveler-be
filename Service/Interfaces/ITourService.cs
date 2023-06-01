using Service.Commons.QueryExtensions;
using Service.Models.Attachment;
using Service.Models.Tour;
using Service.Models.TourVariant;
using Shared.ResultExtensions;

namespace Service.Interfaces;

public interface ITourService
{
    Task<Result<TourDetailsViewModel>> Create(TourCreateModel model);

    Task<Result<TourDetailsViewModel>> Update(Guid id, TourUpdateModel model);

    Task<Result> Delete(Guid id);

    Task<Result<TourDetailsViewModel>> GetDetails(Guid id);

    Task<Result<PaginationModel<TourViewModel>>> Filter(TourFilterModel model);

    // Attachments
    Task<Result<AttachmentViewModel>> AddToCarousel(Guid tourId, string contentType, Stream stream);

    Task<Result> DeleteFromCarousel(Guid tourId, Guid attachmentId);

    Task<Result<List<AttachmentViewModel>>> GetCarousel(Guid tourId);

    Task<Result<List<TourVariantViewModel>>> ListTourVariants(Guid tourId);
}