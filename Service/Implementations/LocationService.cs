using Data.EFCore;
using Data.Entities;
using Mapster;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Service.Interfaces;
using Service.Models.Attachment;
using Service.Models.Location;
using Service.Pagination;
using Shared.Helpers;
using Shared.ResultExtensions;

namespace Service.Implementations;

public class LocationService : BaseService, ILocationService
{
    private readonly ICloudStorageService _cloudStorageService;
    private readonly IAttachmentService _attachmentService;
    private readonly ILogger<LocationService> _logger;
    private readonly IMapper _mapper;

    public LocationService(IUnitOfWork unitOfWork, IMapper mapper, ICloudStorageService cloudStorageService,
        ILogger<LocationService> logger, IAttachmentService attachmentService) : base(unitOfWork)
    {
        _mapper = mapper;
        _cloudStorageService = cloudStorageService;
        _logger = logger;
        _attachmentService = attachmentService;
    }

    public async Task<Result<LocationViewModel>> Create(LocationCreateModel model)
    {
        // Create Location
        var entity = _mapper.Map<Location>(model);
        entity.CreatedAt = DateTimeHelper.VnNow();
        entity = UnitOfWork.Repo<Location>().Add(entity);

        await UnitOfWork.SaveChangesAsync();

        // Result
        var viewModel = _mapper.Map<LocationViewModel>(entity);
        return viewModel;
    }

    public async Task<Result<LocationViewModel>> Update(Guid id, LocationUpdateModel model)
    {
        // Get entity
        var location = await UnitOfWork.Repo<Location>()
            .TrackingQuery()
            .FirstOrDefaultAsync(e => e.Id == id);

        // Update
        if (location is null) return Error.NotFound();

        location = model.Adapt(location, MapperHelper.IgnoreNullConfig<LocationUpdateModel, Location>());

        await UnitOfWork.SaveChangesAsync();

        // Result
        var viewModel = _mapper.Map<LocationViewModel>(location);
        return viewModel;
    }

    public async Task<Result> Delete(Guid id)
    {
        var entity = await UnitOfWork.Repo<Location>().FirstOrDefaultAsync(e => e.Id == id);
        if (entity is null) return Error.NotFound();

        UnitOfWork.Repo<Location>().Remove(entity);

        await UnitOfWork.SaveChangesAsync();

        return Result.Success();
    }

    public async Task<Result<LocationViewModel>> Find(Guid id)
    {
        var entity = await UnitOfWork
            .Repo<Location>()
            .Query()
            .Select(e => new
            {
                e.Id,
                e.Name,
                e.Country,
                e.City,
                e.Address,
                e.Longitude,
                e.Latitude,
                e.Description
            })
            .FirstOrDefaultAsync(e => e.Id == id);

        if (entity is null) return Error.NotFound();

        var viewModel = _mapper.Map<LocationViewModel>(entity);

        return viewModel;
    }

    public async Task<Result<AttachmentViewModel>> CreateAttachment(Guid locationId, string contentType, Stream stream)
    {
        if (!await UnitOfWork.Repo<Location>().AnyAsync(e => e.Id == locationId))
            return Error.NotFound();

        await using var transaction = UnitOfWork.BeginTransaction();

        try
        {
            // Create new Attachment then add to Location
            var createAttachmentResult = await _attachmentService.Create(contentType, stream);

            if (!createAttachmentResult.IsSuccess)
            {
                await transaction.RollbackAsync();
                return Error.Unexpected();
            }

            UnitOfWork.Repo<LocationAttachment>().Add(new LocationAttachment
            {
                LocationId = locationId,
                AttachmentId = createAttachmentResult.Value.Id
            });

            await UnitOfWork.SaveChangesAsync();

            // Success result
            await transaction.CommitAsync();
            return createAttachmentResult.Value;
        }
        catch (Exception e)
        {
            await transaction.RollbackAsync();
            _logger.LogWarning(e, "{Message}", e.Message);
            return Error.Unexpected();
        }
    }

    public async Task<Result> DeleteAttachment(Guid locationId, Guid attachmentId)
    {
        var locationAttachment = await UnitOfWork.Repo<LocationAttachment>()
            .FirstOrDefaultAsync(e => e.LocationId == locationId && e.AttachmentId == attachmentId);

        if (locationAttachment is null) return Error.NotFound();

        await using var transaction = UnitOfWork.BeginTransaction();

        try
        {
            // Remove Attachment - Location
            UnitOfWork.Repo<LocationAttachment>().Remove(locationAttachment);

            // Remove Attachment
            var deleteAttachmentResult = await _attachmentService.Delete(attachmentId);

            if (!deleteAttachmentResult.IsSuccess)
            {
                await transaction.RollbackAsync();
                return Error.Unexpected();
            }

            // Success result
            await transaction.CommitAsync();
            return Result.Success();
        }
        catch (Exception e)
        {
            await transaction.RollbackAsync();
            _logger.LogWarning(e, "{Message}", e.Message);
            return Error.Unexpected();
        }
    }

    public async Task<Result<List<AttachmentViewModel>>> ListAttachments(Guid id)
    {
        if (!await UnitOfWork.Repo<Location>().AnyAsync(e => e.Id == id))
            return Error.NotFound();

        var attachments = await UnitOfWork.Repo<LocationAttachment>()
            .Query()
            .Where(e => e.LocationId == id)
            .Select(e => e.Attachment)
            .ToListAsync();

        return attachments.Select(attachment =>
                new AttachmentViewModel
                {
                    Id = attachment.Id,
                    ContentType = attachment.ContentType,
                    Url = _cloudStorageService.GetMediaLink(attachment.Id)
                })
            .ToList();
    }

    public async Task<Result<PaginationModel<LocationViewModel>>> Filter(LocationFilterModel model)
    {
        IQueryable<Location> query = UnitOfWork.Repo<Location>()
            .Query()
            .OrderByDescending(e => e.CreatedAt);

        if (model.Name != null)
            query = query.Where(e => e.Name.Contains(model.Name));

        var paginationModel = await query.Paging(model.Page, model.Size);

        return paginationModel.UseMapper(e => e.Adapt<List<LocationViewModel>>());
    }
}