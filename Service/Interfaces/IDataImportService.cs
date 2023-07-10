using Service.Models.Tour;
using Service.Models.Trip;
using Shared.ResultExtensions;

namespace Service.Interfaces;

public interface IDataImportService
{
    Task<Result<TourDetailsViewModel>> ImportTour(Stream fileStream);

    Task<Result<TripViewModel>> ImportTrip(Stream fileStream);
}