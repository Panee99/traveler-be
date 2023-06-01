using Data.EFCore;
using Data.Entities;
using Data.Enums;
using Mapster;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Service.Commons;
using Service.Commons.QueryExtensions;
using Service.Interfaces;
using Service.Models.Attachment;
using Service.Models.Tour;
using Service.Models.TourVariant;
using Shared.ResultExtensions;

namespace Service.Implementations;

public class TourService : BaseService, ITourService
{
    private readonly ILogger<TourService> _logger;
    private readonly IMapper _mapper;
    private readonly ICloudStorageService _cloudStorageService;
    private readonly IAttachmentService _attachmentService;

    public TourService(UnitOfWork unitOfWork, ICloudStorageService cloudStorageService, IMapper mapper,
        ILogger<TourService> logger, IAttachmentService attachmentService) :
        base(unitOfWork)
    {
        _cloudStorageService = cloudStorageService;
        _mapper = mapper;
        _logger = logger;
        _attachmentService = attachmentService;
    }

    public async Task<Result<TourDetailsViewModel>> Create(TourCreateModel model)
    {
        // Map
        var tour = model.AdaptIgnoreNull<TourCreateModel, Tour>();
        tour.Status = TourStatus.New;

        // Add
        tour = UnitOfWork.Tours.Add(tour);
        await UnitOfWork.SaveChangesAsync();

        // Add Carousel
        if (model.Carousel != null)
        {
            tour.TourCarousel = model.Carousel.Select(attachmentId => new TourImage()
            {
                TourId = tour.Id,
                AttachmentId = attachmentId
            }).ToList();
        }

        UnitOfWork.Tours.Update(tour);
        await UnitOfWork.SaveChangesAsync();

        // Result
        var view = tour.Adapt<TourDetailsViewModel>();
        if (tour.ThumbnailId != null)
            view.ThumbnailUrl = _cloudStorageService.GetMediaLink(tour.ThumbnailId.Value);

        var tourCarousel = await UnitOfWork.TourCarousel
            .Query()
            .Where(e => e.TourId == tour.Id)
            .Include(e => e.Attachment)
            .ToListAsync();

        view.Carousel = tourCarousel
            .Select(i => i.Attachment)
            .Select(attachment => new AttachmentViewModel()
            {
                Id = attachment.Id,
                ContentType = attachment.ContentType,
                Url = _cloudStorageService.GetMediaLink(attachment.Id)
            })
            .ToList();

        return view;
    }

    public async Task<Result<TourDetailsViewModel>> Update(Guid id, TourUpdateModel model)
    {
        // Get Tour
        var tour = await UnitOfWork.Tours.TrackingQuery()
            .Where(e => e.Id == id)
            .Include(e => e.Schedules)
            .Include(e => e.TourFlows)
            .Include(e => e.TourCarousel)
            .FirstOrDefaultAsync();

        if (tour is null) return Error.NotFound();

        // Update
        model.AdaptIgnoreNull(tour);

        if (model.Carousel != null)
        {
            tour.TourCarousel = model.Carousel.Select(attachmentId => new TourImage()
            {
                TourId = tour.Id,
                AttachmentId = attachmentId
            }).ToList();
        }

        await UnitOfWork.SaveChangesAsync();

        // Result
        var view = tour.Adapt<TourDetailsViewModel>();

        if (tour.ThumbnailId != null)
            view.ThumbnailUrl = _cloudStorageService.GetMediaLink(tour.ThumbnailId.Value);

        var tourCarousel = await UnitOfWork.TourCarousel
            .Query()
            .Where(e => e.TourId == tour.Id)
            .Include(e => e.Attachment)
            .ToListAsync();

        view.Carousel = tourCarousel
            .Select(i => i.Attachment)
            .Select(attachment => new AttachmentViewModel()
            {
                Id = attachment.Id,
                ContentType = attachment.ContentType,
                Url = _cloudStorageService.GetMediaLink(attachment.Id)
            })
            .ToList();

        return view;
    }

    public async Task<Result> Delete(Guid id)
    {
        var entity = await UnitOfWork.Tours.FindAsync(id);
        if (entity is null) return Error.NotFound();

        UnitOfWork.Tours.Remove(entity);
        await UnitOfWork.SaveChangesAsync();

        return Result.Success();
    }

    public async Task<Result<TourDetailsViewModel>> GetDetails(Guid id)
    {
        var tour = await UnitOfWork.Tours.Query()
            .Where(e => e.Id == id)
            .Include(e => e.Schedules)
            .Include(e => e.TourFlows)
            .Include(e => e.TourCarousel).ThenInclude(i => i.Attachment)
            .FirstOrDefaultAsync();

        if (tour is null) return Error.NotFound();

        // Result
        var viewModel = _mapper.Map<TourDetailsViewModel>(tour);

        if (tour.ThumbnailId != null)
            viewModel.ThumbnailUrl = _cloudStorageService.GetMediaLink(tour.ThumbnailId.Value);

        viewModel.Carousel = tour.TourCarousel
            .Select(i => i.Attachment)
            .Select(attachment => new AttachmentViewModel()
            {
                Id = attachment.Id,
                ContentType = attachment.ContentType,
                Url = _cloudStorageService.GetMediaLink(attachment.Id)
            })
            .ToList();

        return viewModel;
    }

    public async Task<Result<PaginationModel<TourViewModel>>> Filter(TourFilterModel model)
    {
        IQueryable<Tour> query = UnitOfWork.Tours.Query()
            .Include(e => e.Schedules)
            .OrderBy(e => e.CreatedAt);

        if (!string.IsNullOrEmpty(model.Title))
            query = query.Where(e => e.Title.Contains(model.Title));
        if (model.Type != null)
            query = query.Where(e => e.Type == model.Type);

        var paginationModel = await query.Paging(model.Page, model.Size);

        return paginationModel.Map(tour =>
        {
            var view = tour.Adapt<TourViewModel>();
            if (tour.ThumbnailId != null)
                view.ThumbnailUrl = _cloudStorageService.GetMediaLink(tour.ThumbnailId.Value);

            return view;
        });
    }

    public async Task<Result<AttachmentViewModel>> AddToCarousel(Guid tourId, string contentType, Stream stream)
    {
        if (!await UnitOfWork.Tours.AnyAsync(e => e.Id == tourId))
            return Error.NotFound();

        await using var transaction = UnitOfWork.BeginTransaction();
        try
        {
            // Add new thumbnail
            var createAttachmentResult = await _attachmentService.Create(contentType, stream);
            if (!createAttachmentResult.IsSuccess)
            {
                await transaction.RollbackAsync();
                return Error.Unexpected();
            }

            var newAttachment = createAttachmentResult.Value;

            UnitOfWork.TourCarousel.Add(new TourImage()
            {
                TourId = tourId,
                AttachmentId = newAttachment.Id
            });

            await UnitOfWork.SaveChangesAsync();

            await transaction.CommitAsync();
            return newAttachment;
        }
        catch (Exception e)
        {
            _logger.LogWarning(e, "{Message}", e);
            await transaction.RollbackAsync();
            return Error.Unexpected();
        }
    }

    public async Task<Result> DeleteFromCarousel(Guid tourId, Guid attachmentId)
    {
        var tourAttachment = await UnitOfWork.TourCarousel.FindAsync(tourId, attachmentId);
        if (tourAttachment is null) return Error.NotFound();

        var transaction = UnitOfWork.BeginTransaction();
        UnitOfWork.TourCarousel.Remove(tourAttachment);
        await UnitOfWork.SaveChangesAsync();

        var deleteResult = await _attachmentService.Delete(attachmentId);
        if (!deleteResult.IsSuccess)
        {
            await transaction.RollbackAsync();
            return Error.Unexpected();
        }

        await transaction.CommitAsync();
        return Result.Success();
    }

    public async Task<Result<List<AttachmentViewModel>>> GetCarousel(Guid tourId)
    {
        if (!await UnitOfWork.Tours.AnyAsync(e => e.Id == tourId))
            return Error.NotFound();

        var attachmentIds = await UnitOfWork.TourCarousel
            .Query()
            .Where(e => e.TourId == tourId)
            .Select(e => e.AttachmentId)
            .ToListAsync();

        var attachments = await UnitOfWork.Attachments
            .Query()
            .Where(e => attachmentIds.Contains(e.Id))
            .ToListAsync();

        return attachments.Select(e => new AttachmentViewModel()
        {
            Id = e.Id,
            ContentType = e.ContentType,
            Url = _cloudStorageService.GetMediaLink(e.Id)
        }).ToList();
    }

    public async Task<Result<List<TourVariantViewModel>>> ListTourVariants(Guid tourId)
    {
        if (!await UnitOfWork.Tours.AnyAsync(e => e.Id == tourId))
            return Error.NotFound("Tour not found.");

        var tourVariants = await UnitOfWork.Tours
            .Query()
            .Where(e => e.Id == tourId)
            .SelectMany(e => e.TourVariants)
            .ToListAsync();

        return tourVariants.Adapt<List<TourVariantViewModel>>();
    }
}