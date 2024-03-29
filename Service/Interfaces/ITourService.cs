﻿using Service.Commons.QueryExtensions;
using Service.Models.Attachment;
using Service.Models.Schedule;
using Service.Models.Tour;
using Service.Models.Trip;
using Shared.ResultExtensions;

namespace Service.Interfaces;

public interface ITourService
{
    Task<Result<TourDetailsViewModel>> ImportTour(Guid createdById, Stream tourZipData);

    Task<Result> Delete(Guid id);

    Task<Result<TourDetailsViewModel>> GetDetails(Guid id);

    Task<Result<PaginationModel<TourViewModel>>> Filter(TourFilterModel model);

    Task<Result<List<TripViewModel>>> ListTrips(Guid tourId);

    Task<Result<List<ScheduleViewModel>>> ListSchedules(Guid tourId);
}