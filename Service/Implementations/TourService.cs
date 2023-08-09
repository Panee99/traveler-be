using Data.EFCore;
using Data.Entities;
using HeyRed.Mime;
using Mapster;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Service.Commons.QueryExtensions;
using Service.ImportHelpers;
using Service.Interfaces;
using Service.Models.Attachment;
using Service.Models.Schedule;
using Service.Models.Tour;
using Service.Models.Trip;
using Shared.Helpers;
using Shared.ResultExtensions;

namespace Service.Implementations;

public class TourService : BaseService, ITourService
{
    private readonly ICloudStorageService _cloudStorageService;
    private readonly ILogger<TourService> _logger;

    public TourService(
        UnitOfWork unitOfWork,
        ICloudStorageService cloudStorageService,
        ILogger<TourService> logger) : base(unitOfWork)
    {
        _cloudStorageService = cloudStorageService;
        _logger = logger;
    }

    public async Task<Result<TourDetailsViewModel>> ImportTour(Guid createdById, Stream tourZipData)
    {
        var transaction = UnitOfWork.BeginTransaction();

        try
        {
            // Read data
            var tourModel = TourImportHelper.ReadTourArchive(tourZipData);
            if (await UnitOfWork.Tours.AnyAsync(e => e.Id == tourModel.Id))
                return Error.Conflict($"Tour '{tourModel.Id}' already exist.");

            // Create tour
            var tour = tourModel.Adapt<Tour>();
            tour.Thumbnail = null;
            tour.CreatedById = createdById;
            tour = UnitOfWork.Tours.Add(tour);
            await UnitOfWork.SaveChangesAsync();

            // Add images to tour
            await _addTourImages(tour.Id, tourModel.Images);
            await _addTourThumbnail(tour.Id, tourModel.Thumbnail);
            await UnitOfWork.SaveChangesAsync();

            // Commit and return
            await transaction.CommitAsync();
            return await GetDetails(tour.Id);
        }
        catch (Exception e)
        {
            await transaction.RollbackAsync();
            _logger.LogError(e, "Import Tour failed: {Message}", e.Message);
            return Error.Validation(e.Message);
        }
    }

    private async Task _addTourImages(Guid tourId, IEnumerable<ImageModel> imageModels)
    {
        // Create attachments
        var images = imageModels.Select(model => (
            Attachment: new Attachment()
            {
                Id = Guid.NewGuid(),
                ContentType = MimeTypesMap.GetMimeType(model.Extension),
                Extension = model.Extension,
                CreatedAt = DateTimeHelper.VnNow()
            },
            model.Data
        )).ToList();

        // Create tour images
        var tourImages = images.Select(img => new TourImage()
        {
            TourId = tourId,
            AttachmentId = img.Attachment.Id
        });

        var attachments = images.Select(img => img.Attachment);

        UnitOfWork.Attachments.AddRange(attachments);
        await UnitOfWork.SaveChangesAsync();
        UnitOfWork.TourImages.AddRange(tourImages);
        await UnitOfWork.SaveChangesAsync();

        // Upload to Cloud
        foreach (var img in images)
        {
            var result = await _cloudStorageService.Upload(
                img.Attachment.FileName,
                img.Attachment.ContentType,
                img.Data
            );

            if (!result.IsSuccess) throw new Exception("Upload tour images failed");
        }
    }

    private async Task _addTourThumbnail(Guid tourId, ImageModel model)
    {
        var tour = await UnitOfWork.Tours.FindAsync(tourId);
        if (tour == null) throw new Exception(DomainErrors.Tour.NotFound);

        var image = (
            Attachment: new Attachment()
            {
                Id = Guid.NewGuid(),
                ContentType = MimeTypesMap.GetMimeType(model.Extension),
                Extension = model.Extension,
                CreatedAt = DateTimeHelper.VnNow()
            },
            model.Data);

        UnitOfWork.Attachments.Add(image.Attachment);
        tour.ThumbnailId = image.Attachment.Id;
        UnitOfWork.Tours.Update(tour);

        await UnitOfWork.SaveChangesAsync();

        // Upload to Cloud
        var result = await _cloudStorageService
            .Upload(image.Attachment.FileName, image.Attachment.ContentType, image.Data);

        if (!result.IsSuccess) throw new Exception("Upload tour thumbnail failed");
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
        var tour = await UnitOfWork.Tours
            .Query()
            .AsSplitQuery()
            .Where(e => e.Id == id)
            .Include(e => e.Schedules).ThenInclude(schedule => schedule.Image)
            .Include(e => e.Thumbnail)
            .Include(e => e.TourCarousel)
            .ThenInclude(x => x.Attachment)
            .FirstOrDefaultAsync();

        if (tour is null) return Error.NotFound();

        // Result
        var viewModel = tour.Adapt<TourDetailsViewModel>();

        viewModel.ThumbnailUrl = _cloudStorageService.GetMediaLink(tour.Thumbnail?.FileName);

        viewModel.Carousel = tour.TourCarousel
            .Select(i => i.Attachment)
            .Select(attachment => new AttachmentViewModel()
            {
                Id = attachment.Id,
                ContentType = attachment.ContentType,
                Url = _cloudStorageService.GetMediaLink(attachment.FileName)
            })
            .ToList();

        viewModel.Schedules = tour.Schedules.Select(schedule =>
        {
            var scheduleView = schedule.Adapt<ScheduleViewModel>();
            scheduleView.ImageUrl = _cloudStorageService.GetMediaLink(schedule.Image?.FileName);
            return scheduleView;
        }).ToList();

        return viewModel;
    }

    public async Task<Result<PaginationModel<TourViewModel>>> Filter(TourFilterModel model)
    {
        IQueryable<Tour> query = UnitOfWork.Tours
            .Query()
            .Include(e => e.Thumbnail)
            .Include(e => e.Schedules)
            .OrderByDescending(e => e.CreatedAt);

        if (!string.IsNullOrEmpty(model.Title))
            query = query.Where(e => e.Title.Contains(model.Title));
        if (model.Type != null)
            query = query.Where(e => e.Type == model.Type);

        var paginationModel = await query.Paging(model.Page, model.Size);

        return paginationModel.Map(tour =>
        {
            var view = tour.Adapt<TourViewModel>();
            view.ThumbnailUrl = _cloudStorageService.GetMediaLink(tour.Thumbnail?.FileName);
            return view;
        });
    }

    public async Task<Result<List<TripViewModel>>> ListTrips(Guid tourId)
    {
        if (!await UnitOfWork.Tours.AnyAsync(e => e.Id == tourId))
            return Error.NotFound(DomainErrors.Tour.NotFound);

        var trips = await UnitOfWork.Tours
            .Query()
            .Where(e => e.Id == tourId)
            .SelectMany(e => e.Trips)
            .ToListAsync();

        return trips.Adapt<List<TripViewModel>>();
    }

    public async Task<Result<List<ScheduleViewModel>>> ListSchedules(Guid tourId)
    {
        if (!await UnitOfWork.Tours.AnyAsync(e => e.Id == tourId))
            return Error.NotFound(DomainErrors.Tour.NotFound);

        var schedules = await UnitOfWork.Schedules
            .Query()
            .Where(e => e.TourId == tourId)
            .Include(e => e.Image)
            .ToListAsync();

        return schedules.Select(schedule =>
        {
            var scheduleView = schedule.Adapt<ScheduleViewModel>();
            scheduleView.ImageUrl = _cloudStorageService.GetMediaLink(schedule.Image?.FileName);
            return scheduleView;
        }).ToList();
    }
}