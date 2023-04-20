using Data.EFCore;
using Data.EFCore.Repositories;
using Data.Entities;
using Data.Enums;
using Mapster;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Service.Commons;
using Service.Interfaces;
using Service.Models.Attachment;
using Service.Models.Location;
using Service.Models.Tour;
using Service.Pagination;
using Shared.Helpers;
using Shared.ResultExtensions;

namespace Service.Implementations;

public class TourService : BaseService, ITourService
{
    private readonly ILogger<TourService> _logger;
    private readonly IMapper _mapper;
    private static readonly Random Random = new();
    private readonly ICloudStorageService _cloudStorageService;
    private readonly IAttachmentService _attachmentService;

    //
    private readonly IRepository<Tour> _tourRepo;
    private readonly IRepository<Location> _locationRepo;
    private readonly IRepository<TourAttachment> _tourAttachmentRepo;
    private readonly IRepository<Attachment> _attachmentRepo;

    public TourService(IUnitOfWork unitOfWork, ICloudStorageService cloudStorageService, IMapper mapper,
        ILogger<TourService> logger, IAttachmentService attachmentService) :
        base(unitOfWork)
    {
        _cloudStorageService = cloudStorageService;
        _mapper = mapper;
        _logger = logger;
        _attachmentService = attachmentService;
        //
        _tourRepo = unitOfWork.Repo<Tour>();
        _locationRepo = unitOfWork.Repo<Location>();
        _attachmentRepo = unitOfWork.Repo<Attachment>();
        _tourAttachmentRepo = unitOfWork.Repo<TourAttachment>();
    }

    public async Task<Result<TourViewModel>> Create(TourCreateModel model)
    {
        var tour = _mapper.Map<Tour>(model);
        tour.Code = await _generateTourCode();
        tour.Status = TourStatus.New;

        tour = _tourRepo.Add(tour);

        var locations = model.Locations.Select(l => new Location()
        {
            TourId = tour.Id,
            Latitude = l.Latitude,
            Longitude = l.Longitude,
            ArrivalTime = l.ArrivalTime
        });

        _locationRepo.AddRange(locations);

        await UnitOfWork.SaveChangesAsync();

        return _mapper.Map<TourViewModel>(tour);
    }

    public async Task<Result<TourViewModel>> Update(Guid id, TourUpdateModel model)
    {
        // Get Tour
        var tour = await _tourRepo.FindAsync(id);
        if (tour is null) return Error.NotFound();

        // Update
        model.AdaptIgnoreNull(tour);

        _tourRepo.Update(tour);
        await UnitOfWork.SaveChangesAsync();

        // Result
        return _mapper.Map<TourViewModel>(tour);
    }

    public async Task<Result> Delete(Guid id)
    {
        var entity = await _tourRepo.FindAsync(id);
        if (entity is null) return Error.NotFound();

        _tourRepo.Remove(entity);
        await UnitOfWork.SaveChangesAsync();

        return Result.Success();
    }

    public async Task<Result<TourViewModel>> Find(Guid id)
    {
        var entity = await _tourRepo.FindAsync(id);
        if (entity is null) return Error.NotFound();

        var viewModel = _mapper.Map<TourViewModel>(entity);

        if (entity.ThumbnailId != null)
            viewModel.ThumbnailUrl = _cloudStorageService.GetMediaLink(entity.ThumbnailId.Value);

        return viewModel;
    }

    public async Task<Result<PaginationModel<TourViewModel>>> Filter(TourFilterModel model)
    {
        IQueryable<Tour> query = _tourRepo.Query().OrderBy(e => e.CreatedAt);

        if (!string.IsNullOrEmpty(model.Title))
            query = query.Where(e => e.Title.Contains(model.Title));
        if (model.Type != null)
            query = query.Where(e => e.Type == model.Type);
        if (model.StartAfter != null)
            query = query.Where(e => e.StartTime >= model.StartAfter);
        if (model.EndBefore != null)
            query = query.Where(e => e.EndTime <= model.EndBefore);
        if (model.MinPrice != null)
            query = query.Where(e => e.AdultPrice >= model.MinPrice);
        if (model.MaxPrice != null)
            query = query.Where(e => e.AdultPrice <= model.MaxPrice);

        var paginationModel = await query.Paging(model.Page, model.Size);

        return paginationModel.Map(x =>
        {
            var filterViewModel = _mapper.Map<TourViewModel>(x);
            if (x.ThumbnailId != null)
                filterViewModel.ThumbnailUrl = _cloudStorageService.GetMediaLink(x.ThumbnailId.Value);

            return filterViewModel;
        });
    }

    /// <summary>
    /// TOUR - LOCATIONS
    /// </summary>
    public async Task<Result<LocationViewModel>> AddLocation(Guid tourId, LocationCreateModel model)
    {
        if (!await _tourRepo.AnyAsync(e => e.Id == tourId)) return Error.NotFound();

        var location = new Location()
        {
            TourId = tourId,
            Longitude = model.Longitude,
            Latitude = model.Latitude,
            ArrivalTime = model.ArrivalTime
        };

        _locationRepo.Add(location);

        await UnitOfWork.SaveChangesAsync();
        return location.Adapt<LocationViewModel>();
    }

    public async Task<Result<LocationViewModel>> UpdateLocation(Guid locationId, LocationUpdateModel model)
    {
        var location = await _locationRepo.FindAsync(locationId);
        if (location is null) return Error.NotFound();

        model.AdaptIgnoreNull(location);

        _locationRepo.Update(location);

        await UnitOfWork.SaveChangesAsync();

        return location.Adapt<LocationViewModel>();
    }

    public async Task<Result> DeleteLocation(Guid id)
    {
        if (!await _locationRepo.AnyAsync(e => e.Id == id)) return Error.NotFound();

        _locationRepo.Remove(new Location() { Id = id });

        await UnitOfWork.SaveChangesAsync();

        return Result.Success();
    }

    public async Task<Result<List<LocationViewModel>>> ListLocations(Guid tourId)
    {
        if (!await _tourRepo.AnyAsync(e => e.Id == tourId)) return Error.NotFound();

        var locations = await _locationRepo
            .Query()
            .Where(e => e.TourId == tourId)
            .ToListAsync();

        return locations.Adapt<List<LocationViewModel>>();
    }

    /// <summary>
    /// ATTACHMENTS
    /// </summary>
    public async Task<Result<AttachmentViewModel>> UpdateThumbnail(Guid tourId, string contentType, Stream stream)
    {
        var tour = await _tourRepo
            .Query()
            .Where(e => e.Id == tourId)
            .Select(e => new Tour()
            {
                Id = tourId,
                ThumbnailId = e.ThumbnailId
            })
            .FirstOrDefaultAsync();

        if (tour is null) return Error.NotFound();
        var oldThumbnailId = tour.ThumbnailId;

        await using var transaction = UnitOfWork.BeginTransaction();
        try
        {
            // Add new thumbnail
            var createThumbnailResult = await _attachmentService.Create(contentType, stream);
            if (!createThumbnailResult.IsSuccess)
            {
                await transaction.RollbackAsync();
                return Error.Unexpected();
            }

            var newThumbnail = createThumbnailResult.Value;

            UnitOfWork.Attach(tour);
            tour.ThumbnailId = newThumbnail.Id;
            await UnitOfWork.SaveChangesAsync();

            // Remove old thumbnail
            if (oldThumbnailId != null)
            {
                var deleteResult = await _attachmentService.Delete(oldThumbnailId.Value);
                if (!deleteResult.IsSuccess)
                {
                    await transaction.RollbackAsync();
                    return Error.Unexpected();
                }
            }

            await transaction.CommitAsync();
            return newThumbnail;
        }
        catch (Exception e)
        {
            _logger.LogWarning(e, "{Message}", e);
            await transaction.RollbackAsync();
            return Error.Unexpected();
        }
    }

    public async Task<Result<AttachmentViewModel>> AddAttachment(Guid tourId, string contentType, Stream stream)
    {
        if (!await _tourRepo.AnyAsync(e => e.Id == tourId))
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

            _tourAttachmentRepo.Add(new TourAttachment()
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

    public async Task<Result> DeleteAttachment(Guid tourId, Guid attachmentId)
    {
        var tourAttachment = await _tourAttachmentRepo.FindAsync(tourId, attachmentId);
        if (tourAttachment is null) return Error.NotFound();

        var transaction = UnitOfWork.BeginTransaction();
        _tourAttachmentRepo.Remove(tourAttachment);
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

    public async Task<Result<List<AttachmentViewModel>>> ListAttachments(Guid tourId)
    {
        if (!await _tourRepo.AnyAsync(e => e.Id == tourId))
            return Error.NotFound();

        var attachmentIds = await _tourAttachmentRepo
            .Query()
            .Where(e => e.TourId == tourId)
            .Select(e => e.AttachmentId)
            .ToListAsync();

        var attachments = await _attachmentRepo
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

    /// <summary>
    /// PRIVATE
    /// </summary>
    private async Task<string> _generateTourCode()
    {
        var now = DateTimeHelper.VnNow();
        var year = now.Year % 1000;
        var month = now.Month < 10 ? "0" + now.Month : now.Month.ToString();

        string code;
        do
        {
            var random = _randomString(5);
            code = $"{random}-{year}{month}";
        } while (await _tourRepo.AnyAsync(e => e.Code == code));

        return code;
    }

    private static string _randomString(int length)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        return new string(Enumerable.Repeat(chars, length)
            .Select(s => s[Random.Next(s.Length)]).ToArray());
    }
}