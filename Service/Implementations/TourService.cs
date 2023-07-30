using System.Data;
using Data.EFCore;
using Data.Entities;
using Data.Enums;
using ExcelDataReader;
using Mapster;
using Microsoft.EntityFrameworkCore;
using Service.Commons.Mapping;
using Service.Commons.QueryExtensions;
using Service.Interfaces;
using Service.Models.Attachment;
using Service.Models.Schedule;
using Service.Models.Tour;
using Service.Models.Trip;
using Shared.ResultExtensions;

namespace Service.Implementations;

public class TourService : BaseService, ITourService
{
    private readonly ICloudStorageService _cloudStorageService;

    public TourService(UnitOfWork unitOfWork, ICloudStorageService cloudStorageService)
        : base(unitOfWork)
    {
        _cloudStorageService = cloudStorageService;
    }

    #region Import Tour

    public async Task<Result<TourDetailsViewModel>> ImportTour(Stream fileStream)
    {
        try
        {
            using var reader = ExcelReaderFactory.CreateReader(fileStream);
            var tour = _readTour(reader);

            if (await UnitOfWork.Tours.AnyAsync(e => e.Id == tour.Id))
                return Error.Conflict($"Tour '{tour.Id}' already exist.");

            UnitOfWork.Tours.Add(tour);
            await UnitOfWork.SaveChangesAsync();

            return await GetDetails(tour.Id);
        }
        catch (Exception e)
        {
            Console.WriteLine(e.StackTrace);
            return Error.Validation(e.Message);
        }
    }

    /// <summary>
    /// Read Tour data from excel file
    /// </summary>
    private static Tour _readTour(IDataReader reader)
    {
        reader.Read(); // skip header
        reader.Read(); // tour data
        var tour = new Tour()
        {
            Id = Guid.Parse(reader.GetString(0)),
            Title = reader.GetString(1),
            Departure = reader.GetString(2),
            Destination = reader.GetString(3),
            Duration = reader.GetString(4),
            Description = reader.GetString(6),
            Policy = reader.GetString(7),
            ThumbnailId = Guid.Parse(reader.GetString(8))
        };

        tour.Type = Enum.Parse<TourType>(reader.GetString(5));

        // Read images
        reader.NextResult();
        tour.TourCarousel = _readImages(reader);

        // Read schedules
        reader.NextResult();
        tour.Schedules = _readSchedules(reader);

        return tour;
    }

    /// <summary>
    /// Read Tour images from excel file
    /// </summary>
    private static ICollection<TourImage> _readImages(IDataReader reader)
    {
        var imageIds = new List<Guid>();

        reader.Read(); // skip header
        while (reader.Read())
        {
            imageIds.Add(Guid.Parse(reader.GetString(0)));
        }

        return imageIds.Select(imageId => new TourImage() { AttachmentId = imageId }).ToList();
    }

    /// <summary>
    /// Read Tour schedules from excel file
    /// </summary>
    private static ICollection<Schedule> _readSchedules(IDataReader reader)
    {
        var schedules = new List<Schedule>();

        reader.Read(); // skip header
        while (reader.Read())
        {
            var schedule = new Schedule()
            {
                Sequence = (int)reader.GetDouble(0),
                Description = reader.GetString(1),
                Longitude = ReferenceEquals(reader.GetValue(2), null) ? null : reader.GetDouble(2),
                Latitude = ReferenceEquals(reader.GetValue(3), null) ? null : reader.GetDouble(3),
                DayNo = (int)reader.GetDouble(4),
            };

            if (!ReferenceEquals(reader.GetString(5), null))
                schedule.Vehicle = Enum.Parse<Vehicle>(reader.GetString(5));

            schedules.Add(schedule);
        }

        return schedules;
    }

    #endregion

    public async Task<Result<TourDetailsViewModel>> Update(Guid id, TourUpdateModel model)
    {
        // Get Tour
        var tour = await UnitOfWork.Tours
            .TrackingQuery()
            .AsSplitQuery()
            .Where(e => e.Id == id)
            .Include(e => e.Schedules)
            .Include(e => e.TourCarousel)
            .Include(e => e.Thumbnail)
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

        view.ThumbnailUrl = _cloudStorageService.GetMediaLink(tour.Thumbnail?.FileName);

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
                Url = _cloudStorageService.GetMediaLink(attachment.FileName)
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
        var tour = await UnitOfWork.Tours
            .Query()
            .AsSplitQuery()
            .Where(e => e.Id == id)
            .Include(e => e.Schedules)
            .Include(e => e.Thumbnail)
            .Include(e => e.TourCarousel)
            .ThenInclude(i => i.Attachment)
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

        return viewModel;
    }

    public async Task<Result<PaginationModel<TourViewModel>>> Filter(TourFilterModel model)
    {
        IQueryable<Tour> query = UnitOfWork.Tours
            .Query()
            .Include(e => e.Thumbnail)
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
            view.ThumbnailUrl = _cloudStorageService.GetMediaLink(tour.Thumbnail?.FileName);
            return view;
        });
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
            Url = _cloudStorageService.GetMediaLink(e.FileName)
        }).ToList();
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

        var entities = await UnitOfWork.Schedules
            .Query()
            .Where(e => e.TourId == tourId)
            .ToListAsync();

        return entities.Adapt<List<ScheduleViewModel>>();
    }
}