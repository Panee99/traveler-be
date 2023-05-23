using Service.Commons.Pagination;
using Service.Models.Attachment;
using Service.Models.Tour;
using Service.Models.TourVariant;
using Shared.ResultExtensions;

namespace Service.Interfaces;

public interface ITourService
{
    Task<Result<TourViewModel>> Create(TourCreateModel model);

    Task<Result<TourViewModel>> Update(Guid id, TourUpdateModel model);

    Task<Result> Delete(Guid id);

    Task<Result<TourViewModel>> GetDetails(Guid id);

    Task<Result<PaginationModel<TourFilterViewModel>>> Filter(TourFilterModel model);

    // Attachments
    Task<Result<AttachmentViewModel>> AddToCarousel(Guid tourId, string contentType, Stream stream);

    Task<Result> DeleteFromCarousel(Guid tourId, Guid attachmentId);

    Task<Result<List<AttachmentViewModel>>> GetCarousel(Guid tourId);

    Task<Result<List<TourVariantViewModel>>> ListTourVariants(Guid tourId);
}