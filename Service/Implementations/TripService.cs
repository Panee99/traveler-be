using System.Data;
using System.Globalization;
using Data.EFCore;
using Data.Entities;
using Data.Enums;
using ExcelDataReader;
using Mapster;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Service.Commons.Mapping;
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

    #region Trip

    public async Task<Result<TripViewModel>> ImportTrip(Stream fileStream)
    {
        // Read data from Excel
        using var reader = ExcelReaderFactory.CreateReader(fileStream);
        var tripModel = ReadTrip(reader);

        // Check if trip already exist
        if (await UnitOfWork.Trips.AnyAsync(e => e.Id == tripModel.Id))
            return Error.Validation($"Trip {tripModel.Id} already exist");

        // Check if tour id exist
        var tour = await UnitOfWork.Tours.FindAsync(tripModel.TourId);
        if (tour is null) return Error.Validation(DomainErrors.Tour.NotFound);

        //
        var trip = new Trip()
        {
            Id = tripModel.Id,
            Code = tripModel.Code,
            StartTime = tripModel.StartTime,
            EndTime = tripModel.EndTime,
            TourId = tripModel.TourId
        };

        var newTourGuides = new List<TourGuide>();
        var newTravelers = new List<Traveler>();
        var travelersInTourGroups = new List<TravelerInTourGroup>();

        await using var transaction = UnitOfWork.BeginTransaction();

        try
        {
            // Handle each group
            foreach (var tourGroupModel in tripModel.TourGroups)
            {
                // Check if tour guide exist
                var tourGuide = await UnitOfWork.TourGuides.Query()
                    .Where(e => e.Email == tourGroupModel.TourGuide.Email)
                    .FirstOrDefaultAsync();

                // Create new tour guide if not exist
                if (tourGuide is null)
                {
                    tourGuide = tourGroupModel.TourGuide.Adapt<TourGuide>();
                    tourGuide.Id = Guid.NewGuid();
                    tourGuide.Password = AuthHelper.HashPassword("123123");
                    newTourGuides.Add(tourGuide);
                }

                // Tour group entity
                var tourGroup = new TourGroup()
                {
                    Id = Guid.NewGuid(),
                    CreatedAt = DateTimeHelper.VnNow(),
                    GroupName = $"{tour.Title} - Group {tourGroupModel.GroupNo}",
                    Status = TourGroupStatus.Prepare,
                    TourGuideId = tourGuide.Id
                };
                trip.TourGroups.Add(tourGroup);

                // Handle group's travelers
                foreach (var travelerModel in tourGroupModel.Travelers)
                {
                    // Check if traveler exist
                    var traveler = await UnitOfWork.Travelers.Query()
                        .Where(e => e.Email == travelerModel.Email)
                        .FirstOrDefaultAsync();

                    // Create new tour guide if not exist
                    if (traveler is null)
                    {
                        traveler = travelerModel.Adapt<Traveler>();
                        traveler.Id = Guid.NewGuid();
                        traveler.Password = AuthHelper.HashPassword("123123");
                        newTravelers.Add(traveler);
                    }

                    // Create traveler and group reference
                    travelersInTourGroups.Add(new TravelerInTourGroup()
                    {
                        TravelerId = traveler.Id,
                        TourGroupId = tourGroup.Id
                    });
                }
            }

            Console.WriteLine(JsonConvert.SerializeObject(trip));

            UnitOfWork.TourGuides.AddRange(newTourGuides);
            UnitOfWork.Travelers.AddRange(newTravelers);
            UnitOfWork.Trips.Add(trip);
            UnitOfWork.TravelersInTourGroups.AddRange(travelersInTourGroups);

            await UnitOfWork.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch (Exception e)
        {
            await transaction.RollbackAsync();
            return Error.Unexpected(e.Message);
        }

        return trip.Adapt<TripViewModel>();
    }

    private static ExcelTripModel ReadTrip(IDataReader reader)
    {
        // Read Trip
        reader.Read(); // skip header
        reader.Read(); // trip data
        var trip = new ExcelTripModel()
        {
            Id = Guid.Parse(reader.GetString(0)),
            TourId = Guid.Parse(reader.GetString(1)),
            Code = reader.GetString(2),
            StartTime = reader.GetDateTime(3),
            EndTime = reader.GetDateTime(4),
        };

        // Read Groups, Users
        reader.NextResult();
        trip.TourGroups = ReadTourGroupModels(reader);

        return trip;
    }

    private static List<ExcelTourGroupModel> ReadTourGroupModels(IDataReader reader)
    {
        var groups = new Dictionary<int, ExcelTourGroupModel>();
        reader.Read(); // skip header
        while (reader.Read())
        {
            var groupNo = (int)reader.GetDouble(6);
            groups.TryAdd(groupNo, new ExcelTourGroupModel() { GroupNo = groupNo });
            var group = groups.GetValueOrDefault(groupNo)!;

            var userModel = new ExcelUserModel
            {
                Phone = reader.GetDouble(0).ToString(CultureInfo.InvariantCulture),
                Email = reader.GetString(1),
                FirstName = reader.GetString(2),
                LastName = reader.GetString(3),
                Gender = Enum.Parse<Gender>(reader.GetString(4)),
                Role = Enum.Parse<UserRole>(reader.GetString(5))
            };

            if (userModel.Role == UserRole.TourGuide)
                group.TourGuide = userModel;
            else if (userModel.Role == UserRole.Traveler)
                group.Travelers.Add(userModel);
        }

        return groups.Values.ToList();
    }

    private sealed class ExcelTripModel
    {
        public Guid Id { get; set; }
        public Guid TourId { get; set; }
        public string Code { get; set; } = null!;
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public ICollection<ExcelTourGroupModel> TourGroups { get; set; } = new List<ExcelTourGroupModel>();
    }

    private sealed class ExcelTourGroupModel
    {
        public int GroupNo { get; set; }
        public ExcelUserModel TourGuide { get; set; } = null!;
        public ICollection<ExcelUserModel> Travelers { get; set; } = new List<ExcelUserModel>();
    }

    private class ExcelUserModel
    {
        public string Phone { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public Gender Gender { get; set; }
        public UserRole Role { get; set; }
    }

    #endregion

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