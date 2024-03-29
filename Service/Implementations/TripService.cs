﻿using Data.EFCore;
using Data.Entities;
using Data.Enums;
using Mapster;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Service.Commons;
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
    private readonly ISmtpService _smtpService;

    public TripService(
        UnitOfWork unitOfWork,
        IHttpContextAccessor httpContextAccessor,
        ISmtpService smtpService) : base(unitOfWork, httpContextAccessor)
    {
        _smtpService = smtpService;
    }

    public async Task<Result<TripViewModel>> ImportTrip(Guid createdById, Guid tourId, Stream tripZipData)
    {
        try
        {
            // Read data from Excel
            var tripModel = TripImportHelper.ReadTripArchive(tripZipData);
            // Validate
            var validateResult = ValidateTripData(tripModel);
            if (!validateResult.IsSuccess) return validateResult.Error;

            // Check if tour id exist
            var tour = await UnitOfWork.Tours.Query()
                .Where(e => e.DeletedById == null)
                .Where(e => e.Id == tourId)
                .FirstOrDefaultAsync();

            if (tour is null) return Error.Validation(DomainErrors.Tour.NotFound);

            // Check if trip StartTime duplicate
            var isDuplicateTime = await UnitOfWork.Trips.Query()
                .Where(trip => trip.DeletedById == null &&
                               trip.Tour.DeletedById == null)
                .Where(trip => trip.TourId == tourId)
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
                TourId = tourId,
                CreatedById = createdById,
                CreatedAt = DateTimeHelper.VnNow()
            };

            UnitOfWork.Trips.Add(trip);

            // List of new users, to send credential emails later
            var newUsers = new List<(string Email, string Name, string Password, string Role)>();

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
                    // var newPassword = AuthHelper.GeneratePassword(8);
                    var newPassword = "123123";
                    user.Password = AuthHelper.HashPassword(newPassword);
                    user.Status = UserStatus.Active;
                    tourGuide = UnitOfWork.TourGuides.Add(user);

                    newUsers.Add((user.Email, $"{user.FirstName} {user.LastName}",
                        newPassword, user.Role.ToString()));
                }
                else if (tourGuide.Role is not UserRole.TourGuide)
                    // If exist, check if user Role
                    return Error.Conflict($"User '{tourGuide.Email}' is not TourGuide");
                else if (await _checkIfUserAlreadyInAnotherTrip(
                             tourGuide.Id, tourGuide.Role, trip.StartTime, trip.EndTime))
                    return Error.Conflict($"User {tourGuide.Email} already in another Trip");

                // Create Group
                var group = new TourGroup()
                {
                    Id = Guid.NewGuid(),
                    TripId = trip.Id,
                    TourGuideId = tourGuide.Id,
                    GroupName = $"{tour.Title} - Group {groupModel.GroupNo}",
                    GroupNo = groupModel.GroupNo,
                    Status = TourGroupStatus.Active,
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
                        // var newPassword = AuthHelper.GeneratePassword(8);
                        var newPassword = "123123";
                        user.Password = AuthHelper.HashPassword(newPassword);
                        user.Status = UserStatus.Active;
                        traveler = UnitOfWork.Travelers.Add(user);

                        newUsers.Add((user.Email, $"{user.FirstName} {user.LastName}",
                            newPassword, user.Role.ToString()));
                    }
                    else if (traveler.Role is not UserRole.Traveler)
                        // If exist, check if user Role
                        return Error.Conflict($"User '{traveler.Email}' is not Traveler");
                    else if (await _checkIfUserAlreadyInAnotherTrip(
                                 traveler.Id, traveler.Role, trip.StartTime, trip.EndTime))
                        return Error.Conflict($"User {traveler.Email} already in another Trip");

                    UnitOfWork.TravelersInTourGroups.Add(new TravelerInTourGroup()
                    {
                        TourGroupId = group.Id,
                        TravelerId = traveler.Id
                    });
                }
            }

            await UnitOfWork.SaveChangesAsync();

            // Send account credentials to new users
            foreach (var user in newUsers)
            {
                _ = Task.Run(() =>
                {
                    _smtpService.MailAccountCredentials(
                        user.Email, user.Name, user.Password, user.Role);
                });
            }

            return await Get(trip.Id);
        }
        catch (Exception e)
        {
            return Error.Validation(e.Message);
        }
    }

    // If user trip time overlap return true
    private async Task<bool> _checkIfUserAlreadyInAnotherTrip(
        Guid userId, UserRole role, DateTime tripStartDate, DateTime tripEndDate)
    {
        var groups = new List<TourGroup>();

        switch (role)
        {
            case UserRole.Traveler:
                // fetch groups that traveler currently in
                groups = await UnitOfWork.Travelers.Query()
                    .Where(traveler => traveler.Id == userId)
                    .SelectMany(traveler => traveler.TourGroups)
                    // Filter out deleted Tour and Trip
                    .Where(group => group.Trip.DeletedById == null &&
                                    group.Trip.Tour.DeletedById == null)
                    .Where(group => group.Status != TourGroupStatus.Ended)
                    .Include(group => group.Trip)
                    .ToListAsync();
                break;

            case UserRole.TourGuide:
                // fetch groups that tour guide currently in
                groups = await UnitOfWork.TourGuides.Query()
                    .Where(guide => guide.Id == userId)
                    .SelectMany(guide => guide.TourGroups)
                    // Filter out deleted Tour and Trip
                    .Where(group => group.Trip.DeletedById == null &&
                                    group.Trip.Tour.DeletedById == null)
                    .Where(group => group.Status != TourGroupStatus.Ended)
                    .Include(group => group.Trip)
                    .ToListAsync();
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(role), role, null);
        }

        // check if trip time overlap
        return groups.Select(g => g.Trip)
            .Any(trip => CheckTripTimeOverlap(
                trip.StartTime, trip.EndTime, tripStartDate, tripEndDate));
    }

    private static bool CheckTripTimeOverlap(
        DateTime startDate1, DateTime endDate1,
        DateTime startDate2, DateTime endDate2)
    {
        return endDate1 >= startDate2 && endDate2 >= startDate1;
    }

    private static Result ValidateTripData(TripImportHelper.TripModel tripModel)
    {
        if (tripModel.StartTime > tripModel.EndTime)
            return Error.Validation("StartTime must not greater than EndTime");

        if (tripModel.StartTime.Date < DateTimeHelper.VnNow().Date)
            return Error.Validation("StartTime must not less than Today");

        foreach (var group in tripModel.TourGroups)
        {
            if (group.TourGuide.FirstName.Length >= 64 || group.TourGuide.LastName.Length >= 64)
                return Error.Validation("First Name, Last Name length must < 64");
            if (group.TourGuide.Email.Length >= 128)
                return Error.Validation("Email length must < 128");

            foreach (var traveler in group.Travelers)
            {
                if (traveler.FirstName.Length >= 64 || traveler.LastName.Length >= 64)
                    return Error.Validation("First Name, Last Name length must < 64");

                if (traveler.Email.Length >= 128)
                    return Error.Validation("Email length must < 128");
            }
        }

        //
        return Result.Success();
    }

    // public async Task<Result<TripViewModel>> Update(Guid id, TripUpdateModel model)
    // {
    //     var trip = await UnitOfWork.Trips.Query()
    //         .Where(e => e.DeletedById == null)
    //         .Where(e => e.Id == id)
    //         .FirstOrDefaultAsync();
    //
    //     if (trip is null) return Error.NotFound();
    //
    //     model.AdaptIgnoreNull(trip);
    //     UnitOfWork.Trips.Update(trip);
    //
    //     await UnitOfWork.SaveChangesAsync();
    //     return trip.Adapt<TripViewModel>();
    // }

    public async Task<Result<TripViewModel>> Get(Guid id)
    {
        var trip = await UnitOfWork.Trips.Query()
            .Where(e => e.DeletedById == null)
            .FirstOrDefaultAsync(e => e.Id == id);

        if (trip is null) return Error.NotFound();
        return trip.Adapt<TripViewModel>();
    }

    public async Task<Result> Delete(Guid id)
    {
        var trip = await UnitOfWork.Trips.Query()
            .Where(e => e.DeletedById == null)
            .FirstOrDefaultAsync(e => e.Id == id);

        if (trip is null) return Error.NotFound();

        if (CurrentUser is null) return Error.Authentication();
        trip.DeletedById = CurrentUser.Id;

        UnitOfWork.Trips.Update(trip);
        await UnitOfWork.SaveChangesAsync();

        return Result.Success();
    }

    public async Task<Result<List<TourGroupViewModel>>> ListGroupsInTrip(Guid tripId)
    {
        if (!await UnitOfWork.Trips.AnyAsync(e => e.DeletedById == null && e.Id == tripId)) return Error.NotFound();

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
        if (!await UnitOfWork.Trips.AnyAsync(e => e.DeletedById == null && e.Id == tripId))
            return Error.NotFound(DomainErrors.Trip.NotFound);

        var alerts = await UnitOfWork.WeatherAlerts
            .Query()
            .Where(e => e.TripId == tripId)
            .ToListAsync();

        return alerts.Adapt<List<WeatherAlertViewModel>>();
    }
}