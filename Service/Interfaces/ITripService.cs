﻿using Service.Models.TourGroup;
using Service.Models.Trip;
using Service.Models.Weather;
using Shared.ResultExtensions;

namespace Service.Interfaces;

public interface ITripService
{
    // Task<Result<TripViewModel>> Create(TripCreateModel model);

    Task<Result<TripViewModel>> Update(Guid id, TripUpdateModel model);

    Task<Result<TripViewModel>> Get(Guid id);

    Task<Result> Delete(Guid id);

    Task<Result<List<TourGroupViewModel>>> ListGroupsInTrip(Guid tripId);

    Task<Result<List<WeatherAlertViewModel>>> GetWeatherAlerts(Guid tripId);
}