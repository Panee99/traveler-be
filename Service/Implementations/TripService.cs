using Data.EFCore;
using Data.Entities;
using Data.Enums;
using Mapster;
using Microsoft.EntityFrameworkCore;
using Service.Commons;
using Service.Commons.Mapping;
using Service.ImportHelpers;
using Service.Interfaces;
using Service.Models.TourGroup;
using Service.Models.Trip;
using Service.Models.Weather;
using Shared.Helpers;
using Shared.ResultExtensions;

namespace Service.Implementations;

public class TripService : BaseService, ITripService
{
    public TripService(UnitOfWork unitOfWork) : base(unitOfWork)
    {
    }

    public async Task<Result<TripViewModel>> ImportTrip(Guid createdById, Stream tripZipData)
    {
        // Read data from Excel
        var tripModel = TripImportHelper.ReadTripArchive(tripZipData);

        // Check if tour id exist
        var tour = await UnitOfWork.Tours.FindAsync(tripModel.TourId);
        if (tour is null) return Error.Validation(DomainErrors.Tour.NotFound);

        // Check if trip StartTime duplicate
        var isDuplicateTime = await UnitOfWork.Trips.Query()
            .Where(trip => trip.TourId == tripModel.TourId)
            .AnyAsync(trip => trip.StartTime == tripModel.StartTime);

        if (isDuplicateTime)
            return Error.Conflict($"Trip StartTime duplicate: {DateOnly.FromDateTime(tripModel.StartTime)}");

        // Create Trip entity
        var trip = new Trip()
        {
            Id = Guid.NewGuid(),
            Code = CodeGenerator.NewCode(),
            StartTime = tripModel.StartTime,
            EndTime = tripModel.EndTime,
            TourId = tripModel.TourId,
            CreatedById = createdById
        };

        UnitOfWork.Trips.Add(trip);

        // Create Group entities
        foreach (var groupModel in tripModel.TourGroups)
        {
            // Find tour guide in DB
            var tourGuide = await UnitOfWork.Users.Query()
                .Where(e => e.Email == groupModel.TourGuide.Email)
                .FirstOrDefaultAsync();

            if (tourGuide is null)
            {
                // Add to DB if tour guide not exist
                var user = groupModel.TourGuide.Adapt<TourGuide>();
                user.Password = AuthHelper.HashPassword("123123");
                user.Status = UserStatus.Active;
                tourGuide = UnitOfWork.TourGuides.Add(user);
            }

            else if (tourGuide.Role is not UserRole.TourGuide)
                // If exist, check if user Role
                return Error.Conflict($"User '{tourGuide.Email}' is not TourGuide");

            var group = new TourGroup()
            {
                Id = Guid.NewGuid(),
                TripId = trip.Id,
                TourGuideId = tourGuide.Id,
                GroupName = "...",
                GroupNo = groupModel.GroupNo,
                Status = TourGroupStatus.Prepare,
                CreatedAt = DateTimeHelper.VnNow(),
            };

            UnitOfWork.TourGroups.Add(group);

            foreach (var travelerModel in groupModel.Travelers)
            {
                // Find traveler in DB
                var traveler = await UnitOfWork.Users.Query()
                    .Where(e => e.Email == travelerModel.Email)
                    .FirstOrDefaultAsync();

                if (traveler is null)
                {
                    // Add to DB if traveler not exist
                    var user = travelerModel.Adapt<Traveler>();
                    user.Password = AuthHelper.HashPassword("123123");
                    user.Status = UserStatus.Active;
                    traveler = UnitOfWork.Travelers.Add(user);
                    Console.WriteLine(traveler.Id);
                }

                else if (traveler.Role is not UserRole.Traveler)
                    // If exist, check if user Role
                    return Error.Conflict($"User '{traveler.Email}' is not Traveler");

                UnitOfWork.TravelersInTourGroups.Add(new TravelerInTourGroup()
                {
                    TourGroupId = group.Id,
                    TravelerId = traveler.Id
                });
            }
        }

        await UnitOfWork.SaveChangesAsync();
        return await Get(trip.Id);
    }

    public async Task<Result<TripViewModel>> Update(Guid id, TripUpdateModel model)
    {
        var trip = await UnitOfWork.Trips.FindAsync(id);
        if (trip is null) return Error.NotFound();

        model.AdaptIgnoreNull(trip);
        UnitOfWork.Trips.Update(trip);

        await UnitOfWork.SaveChangesAsync();
        return trip.Adapt<TripViewModel>();
    }

    public async Task<Result<TripViewModel>> Get(Guid id)
    {
        var trip = await UnitOfWork.Trips.FindAsync(id);
        if (trip is null) return Error.NotFound();
        return trip.Adapt<TripViewModel>();
    }

    public async Task<Result> Delete(Guid id)
    {
        var trip = await UnitOfWork.Trips.FindAsync(id);
        if (trip is null) return Error.NotFound();

        UnitOfWork.Trips.Remove(trip);
        await UnitOfWork.SaveChangesAsync();

        return Result.Success();
    }

    public async Task<Result<List<TourGroupViewModel>>> ListGroupsInTrip(Guid tripId)
    {
        if (!await UnitOfWork.Trips.AnyAsync(e => e.Id == tripId)) return Error.NotFound();

        var groupResults = await UnitOfWork.TourGroups
            .Query()
            .Where(e => e.TripId == tripId)
            .Select(e => new { Group = e, TravelerCount = e.Travelers.Count })
            .ToListAsync();

        // return
        return groupResults.Select(groupResult =>
        {
            var view = groupResult.Group.Adapt<TourGroupViewModel>();
            view.TravelerCount = groupResult.TravelerCount;
            return view;
        }).ToList();
    }

    public async Task<Result<List<WeatherAlertViewModel>>> ListWeatherAlerts(Guid tripId)
    {
        if (!await UnitOfWork.Trips.AnyAsync(e => e.Id == tripId))
            return Error.NotFound(DomainErrors.Trip.NotFound);

        var alerts = await UnitOfWork.WeatherAlerts
            .Query()
            .Where(e => e.TripId == tripId)
            .ToListAsync();

        return alerts.Adapt<List<WeatherAlertViewModel>>();
    }

    public async Task<Result<List<WeatherForecastViewModel>>> ListWeatherForecasts(Guid tripId)
    {
        if (!await UnitOfWork.Trips.AnyAsync(e => e.Id == tripId))
            return Error.NotFound(DomainErrors.Trip.NotFound);

        var forecasts = await UnitOfWork.WeatherForecasts
            .Query()
            .Where(e => e.TripId == tripId)
            .ToListAsync();

        return forecasts.Adapt<List<WeatherForecastViewModel>>();
    }
}