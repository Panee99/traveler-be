using Data.EFCore;
using Data.Entities;
using Data.Enums;
using Mapster;
using Microsoft.EntityFrameworkCore;
using Service.Commons;
using Service.Commons.Mapping;
using Service.Interfaces;
using Service.Models.TourGroup;
using Service.Models.Trip;
using Shared.ResultExtensions;

namespace Service.Implementations;

public class TripService : BaseService, ITripService
{
    public TripService(UnitOfWork unitOfWork) : base(unitOfWork)
    {
    }

    // public async Task<Result<TripViewModel>> Create(TripCreateModel model)
    // {
    //     var trip = model.Adapt<Trip>();
    //     trip.Code = CodeGenerator.NewCode();
    //
    //     UnitOfWork.Trips.Add(trip);
    //     await UnitOfWork.SaveChangesAsync();
    //
    //     return trip.Adapt<TripViewModel>();
    // }

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
}