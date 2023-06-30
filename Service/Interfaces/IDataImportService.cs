using Service.Models.Tour;
using Shared.ResultExtensions;

namespace Service.Interfaces;

public interface IDataImportService
{
    Task<Result<TourDetailsViewModel>> ImportTour(Stream fileStream);

    // Task<Result> TripTour(Stream openReadStream);
}