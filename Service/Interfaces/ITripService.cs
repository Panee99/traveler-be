﻿using Service.Models.TourGroup;
using Service.Models.Trip;
using Service.Models.Weather;
using Shared.ResultExtensions;

namespace Service.Interfaces;

public interface ITripService
{
    Task<Result<TripViewModel>> ImportTrip(Guid createdById, Guid tourId, Stream tripZipData);

    Task<Result<TripViewModel>> Get(Guid id);

    Task<Result> Delete(Guid id);

    Task<Result<List<TourGroupViewModel>>> ListGroupsInTrip(Guid tripId);

    Task<Result<List<WeatherAlertViewModel>>> ListWeatherAlerts(Guid tripId);
}