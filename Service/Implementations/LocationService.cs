using Data.EFCore;
using Data.Entities;
using Mapster;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Service.Interfaces;
using Service.Models.Attachment;
using Service.Models.Location;
using Service.Models.Tag;
using Service.Pagination;
using Shared.Helpers;
using Shared.ResultExtensions;

namespace Service.Implementations;

public class LocationService : BaseService, ILocationService
{
    private readonly ICloudStorageService _cloudStorageService;
    private readonly ILogger<LocationService> _logger;
    private readonly IMapper _mapper;

    public LocationService(IUnitOfWork unitOfWork, IMapper mapper, ICloudStorageService cloudStorageService,
        ILogger<LocationService> logger) : base(unitOfWork)
    {
        _mapper = mapper;
        _cloudStorageService = cloudStorageService;
        _logger = logger;
    }

    public async Task<Result<LocationViewModel>> Create(LocationCreateModel model)
    {
        // Create Location
        var entity = _mapper.Map<Location>(model);
        entity.CreatedAt = DateTimeHelper.VnNow();
        entity = UnitOfWork.Repo<Location>().Add(entity);

        // Add Tags
        if (model.Tags is { Count: > 0 })
            UnitOfWork.Repo<LocationTag>().AddRange(
                model.Tags.Select(id => new LocationTag
                    {
                        LocationId = entity.Id,
                        TagId = id
                    }
                )
            );

        await UnitOfWork.SaveChangesAsync();

        // Result
        var tags = new List<Tag>();
        if (model.Tags is { Count: > 0 })
            tags = await UnitOfWork.Repo<Tag>().Query().Where(e => model.Tags.Contains(e.Id)).ToListAsync();

        var viewModel = _mapper.Map<LocationViewModel>(entity);
        viewModel.Tags = _mapper.Map<List<TagViewModel>>(tags);
        return viewModel;
    }

    public async Task<Result<LocationViewModel>> Update(Guid id, LocationUpdateModel model)
    {
        // Get entity
        var location = await UnitOfWork.Repo<Location>()
            .TrackingQuery()
            .Include(e => e.LocationTags)
            .FirstOrDefaultAsync(e => e.Id == id);

        // Update
        if (location is null) return Error.NotFound();

        location = model.Adapt(location, MapperHelper.IgnoreNullConfig<LocationUpdateModel, Location>());

        if (model.Tags != null)
            location.LocationTags = model.Tags.Select(t =>
                new LocationTag
                {
                    LocationId = location.Id,
                    TagId = t
                }
            ).ToList();

        await UnitOfWork.SaveChangesAsync();

        // Result
        var tags = await UnitOfWork.Repo<Location>().Query()
            .Where(e => e.Id == id)
            .SelectMany(e => e.LocationTags)
            .Select(lt => lt.Tag)
            .ToListAsync();

        var viewModel = _mapper.Map<LocationViewModel>(location);
        viewModel.Tags = _mapper.Map<List<TagViewModel>>(tags);
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
                e.Description,
                Tags = e.LocationTags.Select(lt => lt.Tag).ToList()
            })
            .FirstOrDefaultAsync(e => e.Id == id);

        if (entity is null) return Error.NotFound();

        var viewModel = _mapper.Map<LocationViewModel>(entity);
        viewModel.Tags = _mapper.Map<List<TagViewModel>>(entity.Tags);

        return viewModel;
    }

    public async Task<Result<AttachmentViewModel>> CreateAttachment(Guid locationId, AttachmentCreateModel model)
    {
        await using var transaction = UnitOfWork.BeginTransaction();

        try
        {
            // Validate
            if (!await UnitOfWork.Repo<Location>().AnyAsync(e => e.Id == locationId))
                return Error.NotFound();

            // Create new attachment then add to location
            var attachment = UnitOfWork.Repo<Attachment>().Add(new Attachment
            {
                ContentType = model.ContentType,
                CreatedAt = DateTimeHelper.VnNow()
            });

            UnitOfWork.Repo<LocationAttachment>().Add(new LocationAttachment
            {
                LocationId = locationId,
                AttachmentId = attachment.Id
            });

            await UnitOfWork.SaveChangesAsync();

            // Upload to cloud storage
            var uploadResult = await _cloudStorageService.Upload(attachment.Id, attachment.ContentType, model.Stream);

            if (!uploadResult.IsSuccess)
            {
                await transaction.RollbackAsync();
                return Error.Unexpected();
            }

            await transaction.CommitAsync();

            // Success result
            return new AttachmentViewModel
            {
                Id = attachment.Id,
                ContentType = attachment.ContentType,
                Url = uploadResult.Value
            };
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
        await using var transaction = UnitOfWork.BeginTransaction();

        try
        {
            // Validate
            var attachment = await UnitOfWork.Repo<Attachment>().FirstOrDefaultAsync(a => a.Id == attachmentId);
            if (attachment is null) return Error.NotFound();

            var locationAttachment = await UnitOfWork.Repo<LocationAttachment>()
                .FirstOrDefaultAsync(e => e.LocationId == locationId && e.AttachmentId == attachmentId);
            if (locationAttachment is null) return Error.NotFound();

            // Remove old attachment in DB
            UnitOfWork.Repo<LocationAttachment>().Remove(locationAttachment);
            UnitOfWork.Repo<Attachment>().Remove(attachment);

            await UnitOfWork.SaveChangesAsync();

            // Delete old attachment in cloud storage
            var storageResult = await _cloudStorageService.Delete(attachment.Id);
            if (!storageResult.IsSuccess)
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
        IQueryable<Location> query;
        
        // NO TAGS
        if (model.Tags is null || model.Tags.Count == 0) 
            query = UnitOfWork.Repo<Location>().Query().OrderByDescending(e => e.CreatedAt);
        
        // WITH TAGS
        else
        {
            // Get location ids that have one of those tag
            var validLocations = UnitOfWork.Repo<LocationTag>()
                .Query()
                .Where(lt => model.Tags.Contains(lt.TagId))
                .Select(lt => lt.LocationId);

            // Get location entities
            query = UnitOfWork.Repo<Location>()
                .Query()
                .OrderByDescending(l => l.CreatedAt)
                .Where(l => validLocations.Contains(l.Id));
        }
        
        // FINALIZE
        if (!string.IsNullOrEmpty(model.Name))
            query = query.Where(e => e.Name.Contains(model.Name));
        
        var paginationModel = await query
            .Include(e => e.LocationTags)
            .ThenInclude(lt => lt.Tag)
            .Paging(model.Page, model.Size);
        
        var mapperConfig = new TypeAdapterConfig()
            .NewConfig<Location, LocationViewModel>()
            .Map(
                dest => dest.Tags, 
                src => src.LocationTags.Select(lt => lt.Tag).Adapt<List<TagViewModel>>())
            .Config;
        
        return paginationModel.UseMapper(e => e.Adapt<List<LocationViewModel>>(mapperConfig));
    }
}