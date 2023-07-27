using System.Data;
using System.Globalization;
using Data.EFCore;
using Data.Entities;
using Data.Enums;
using ExcelDataReader;
using Mapster;
using Microsoft.EntityFrameworkCore;
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
        using var reader = ExcelReaderFactory.CreateReader(fileStream);
        var tripModel = ReadTrip(reader);

        var tour = await UnitOfWork.Tours.FindAsync(tripModel.TourId);
        if (tour is null) return Error.Validation(DomainErrors.Tour.NotFound);

        var trip = tripModel.Adapt<Trip>();

        await using var transaction = UnitOfWork.BeginTransaction();

        try
        {
            foreach (var tourGroupModel in tripModel.TourGroups)
            {
                // Add tour guide if not exist
                var tourGuide = await UnitOfWork.TourGuides
                    .Query()
                    .Where(e => e.Email == tourGroupModel.TourGuide.Email)
                    .FirstOrDefaultAsync();

                if (tourGuide is null)
                {
                    tourGuide = tourGroupModel.TourGuide.Adapt<TourGuide>();
                    tourGuide.Password = AuthHelper.HashPassword("123123");
                    UnitOfWork.TourGuides.Add(tourGuide);
                    await UnitOfWork.SaveChangesAsync();
                }

                // Add travelers if not exist
                var travelers = new List<Traveler>();

                foreach (var travelerModel in tourGroupModel.Travelers)
                {
                    var traveler = await UnitOfWork.Travelers
                        .Query()
                        .Where(e => e.Email == travelerModel.Email)
                        .FirstOrDefaultAsync();

                    if (traveler is null)
                    {
                        traveler = travelerModel.Adapt<Traveler>();
                        traveler.Password = AuthHelper.HashPassword("123123");
                        UnitOfWork.Travelers.Add(traveler);
                        await UnitOfWork.SaveChangesAsync();
                    }

                    travelers.Add(traveler);
                }

                // Add tour group to trip
                trip.TourGroups.Add(new TourGroup()
                {
                    // TourGuide = tourGuide,
                    // Travelers = travelers,
                    CreatedAt = DateTime.Now,
                    GroupName = $"{tour.Title} - Group {tourGroupModel.GroupNo}",
                });
            }

            UnitOfWork.Trips.Add(trip);
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

            var userModel = new ExcelUserModel()
            {
                Phone = reader.GetDouble(0).ToString(CultureInfo.InvariantCulture),
                Email = reader.GetString(1),
                FirstName = reader.GetString(2),
                LastName = reader.GetString(3),
            };

            userModel.Gender = Enum.Parse<Gender>(reader.GetString(4));
            userModel.Role = Enum.Parse<UserRole>(reader.GetString(5));

            if (userModel.Role == UserRole.TourGuide)
                group.TourGuide = userModel;
            else if (userModel.Role == UserRole.Traveler)
                group.Travelers.Add(userModel);
        }

        return groups.Values.ToList();
    }

    private class ExcelTripModel
    {
        public Guid Id { get; set; }
        public Guid TourId { get; set; }
        public string Code { get; set; } = null!;
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public virtual ICollection<ExcelTourGroupModel> TourGroups { get; set; } = new List<ExcelTourGroupModel>();
    }

    private class ExcelTourGroupModel
    {
        public int GroupNo { get; set; }
        public ExcelUserModel TourGuide { get; set; } = null!;
        public virtual ICollection<ExcelUserModel> Travelers { get; set; } = new List<ExcelUserModel>();
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
}