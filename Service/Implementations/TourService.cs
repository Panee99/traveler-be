using Data.EFCore;
using Data.Entities;
using Mapster;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Service.Interfaces;
using Service.Models.Attachment;
using Service.Models.Tour;
using Service.Pagination;
using Shared.Helpers;
using Shared.ResultExtensions;

namespace Service.Implementations;

public class TourService : BaseService, ITourService
{
    // PRIVATE
    private static readonly Random Random = new();
    private readonly ICloudStorageService _cloudStorageService;
    private readonly ILogger<TourService> _logger;
    private readonly IMapper _mapper;

    public TourService(IUnitOfWork unitOfWork, ICloudStorageService cloudStorageService, IMapper mapper,
        ILogger<TourService> logger) :
        base(unitOfWork)
    {
        _cloudStorageService = cloudStorageService;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<TourViewModel>> Create(TourCreateModel model)
    {
        var tour = _mapper.Map<Tour>(model);
        tour.Code = await _generateTourCode();
        tour.CreatedAt = DateTimeHelper.VnNow();

        tour = UnitOfWork.Repo<Tour>().Add(tour);
        await UnitOfWork.SaveChangesAsync();

        return _mapper.Map<TourViewModel>(tour);
    }


    public async Task<Result<TourViewModel>> Update(Guid id, TourUpdateModel model)
    {
        // Get Tour
        var tour = await UnitOfWork.Repo<Tour>().FirstOrDefaultAsync(e => e.Id == id);
        if (tour is null) return Error.NotFound();

        // Update
        tour = model.Adapt(tour, MapperHelper.IgnoreNullConfig<TourUpdateModel, Tour>());

        UnitOfWork.Repo<Tour>().Update(tour);
        await UnitOfWork.SaveChangesAsync();

        // Result
        return _mapper.Map<TourViewModel>(tour);
    }

    public async Task<Result> Delete(Guid id)
    {
        var entity = await UnitOfWork.Repo<Tour>().FirstOrDefaultAsync(e => e.Id == id);
        if (entity is null) return Error.NotFound();

        UnitOfWork.Repo<Tour>().Remove(entity);
        await UnitOfWork.SaveChangesAsync();

        return Result.Success();
    }

    public async Task<Result<TourViewModel>> Find(Guid id)
    {
        var entity = await UnitOfWork.Repo<Tour>().FirstOrDefaultAsync(e => e.Id == id);
        if (entity is null) return Error.NotFound();

        var viewModel = _mapper.Map<TourViewModel>(entity);

        if (entity.ThumbnailId != null)
            viewModel.ThumbnailUrl = _cloudStorageService.GetMediaLink(entity.ThumbnailId.Value);

        return viewModel;
    }

    public async Task<Result<AttachmentViewModel>> UpdateThumbnail(Guid id, string contentType, Stream stream)
    {
        await using var transaction = UnitOfWork.BeginTransaction();

        try
        {
            // Find tour
            var tour = await UnitOfWork.Repo<Tour>()
                .TrackingQuery()
                .Include(e => e.Thumbnail)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (tour is null) return Error.NotFound();

            var oldThumbnail = tour.Thumbnail;

            // Add new thumbnail
            var newThumbnail = UnitOfWork.Repo<Attachment>().Add(
                new Attachment
                {
                    ContentType = contentType,
                    CreatedAt = DateTimeHelper.VnNow()
                });

            tour.Thumbnail = newThumbnail;

            // Remove old thumbnail
            if (oldThumbnail != null)
            {
                UnitOfWork.Repo<Attachment>().Remove(oldThumbnail);
                var deleteResult = await _cloudStorageService.Delete(oldThumbnail.Id);
                if (!deleteResult.IsSuccess)
                {
                    await transaction.RollbackAsync();
                    return Error.Unexpected();
                }
            }

            await UnitOfWork.SaveChangesAsync();

            // Upload new thumbnail to Cloud
            var uploadResult = await _cloudStorageService.Upload(newThumbnail.Id, newThumbnail.ContentType, stream);
            if (!uploadResult.IsSuccess)
            {
                await transaction.RollbackAsync();
                return Error.Unexpected();
            }

            await transaction.CommitAsync();

            // Result
            return new AttachmentViewModel
            {
                Id = newThumbnail.Id,
                ContentType = newThumbnail.ContentType,
                Url = _cloudStorageService.GetMediaLink(newThumbnail.Id)
            };
        }
        catch (Exception e)
        {
            _logger.LogWarning(e, "{Message}", e);
            await transaction.RollbackAsync();
            return Error.Unexpected();
        }
    }

    public async Task<Result<PaginationModel<TourFilterViewModel>>> Filter(TourFilterModel model)
    {
        IQueryable<Tour> query = UnitOfWork.Repo<Tour>().Query().OrderBy(e => e.CreatedAt);

        if (!string.IsNullOrEmpty(model.Title))
            query = query.Where(e => e.Title.Contains(model.Title));
        if (model.Type != null)
            query = query.Where(e => e.Type == model.Type);
        if (model.StartAfter != null)
            query = query.Where(e => e.StartTime >= model.StartAfter);
        if (model.EndBefore != null)
            query = query.Where(e => e.EndTime <= model.EndBefore);
        if (model.MinPrice != null)
            query = query.Where(e => e.Price >= model.MinPrice);
        if (model.MaxPrice != null)
            query = query.Where(e => e.Price <= model.MaxPrice);

        var paginationModel = await query.Select(e => new
            {
                e.Id,
                e.Title,
                e.Price,
                e.Description,
                e.StartTime,
                e.EndTime,
                e.Type,
                e.ThumbnailId
            })
            .Paging(model.Page, model.Size);

        return paginationModel.Map(x =>
        {
            var filterViewModel = _mapper.Map<TourFilterViewModel>(x);
            if (x.ThumbnailId != null)
                filterViewModel.ThumbnailUrl = _cloudStorageService.GetMediaLink(x.ThumbnailId.Value);

            return filterViewModel;
        });
    }

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
        } while (await UnitOfWork.Repo<Tour>().AnyAsync(e => e.Code == code));

        return code;
    }

    private static string _randomString(int length)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        return new string(Enumerable.Repeat(chars, length)
            .Select(s => s[Random.Next(s.Length)]).ToArray());
    }
}