using Data.EFCore;
using Data.Entities;
using Mapster;
using Microsoft.EntityFrameworkCore;
using Service.Interfaces;
using Service.Models.TourGroup;
using Shared.ResultExtensions;

namespace Service.Implementations;

public class TourGroupService : BaseService, ITourGroupService
{
    public TourGroupService(UnitOfWork unitOfWork) : base(unitOfWork)
    {
    }

    public async Task<Result<TourGroupViewModel>> Create(TourGroupCreateModel model)
    {
        var tour = await UnitOfWork.Tours.FindAsync(model.TourId);
        if (tour is null) return Error.NotFound();

        var group = UnitOfWork.TourGroups.Add(new TourGroup
        {
            TourId = tour.Id,
            GroupName = model.GroupName
        });

        await UnitOfWork.SaveChangesAsync();

        return group.Adapt<TourGroupViewModel>();
    }

    public async Task<Result<TourGroupViewModel>> Update(Guid groupId, TourGroupUpdateModel model)
    {
        var group = await UnitOfWork.TourGroups.FindAsync(groupId);
        if (group is null) return Error.NotFound("Tour group not found.");

        if (model.GroupName != null) group.GroupName = model.GroupName;

        if (model.TourGuideId != null)
        {
            var tourGuide = await UnitOfWork.TourGuides.FindAsync(model.TourGuideId);
            if (tourGuide is null) return Error.NotFound("Tour guide not found.");
            group.TourGuide = tourGuide;
        }

        UnitOfWork.TourGroups.Update(group);

        await UnitOfWork.SaveChangesAsync();

        return group.Adapt<TourGroupViewModel>();
    }

    public async Task<Result> Delete(Guid groupId)
    {
        var group = await UnitOfWork.TourGroups.FindAsync(groupId);
        if (group is null) return Error.NotFound();

        UnitOfWork.TourGroups.Remove(group);
        await UnitOfWork.SaveChangesAsync();
        return Result.Success();
    }

    public async Task<Result<List<TourGroupViewModel>>> ListGroupsByTour(Guid tourId)
    {
        if (!await UnitOfWork.Tours.AnyAsync(e => e.Id == tourId)) return Error.NotFound();

        var tourGroups = await UnitOfWork.TourGroups
            .Query()
            .Where(e => e.TourId == tourId)
            .ToListAsync();

        return tourGroups.Adapt<List<TourGroupViewModel>>();
    }

    public async Task<Result> AddTravelers(Guid tourGroupId, ICollection<Guid> travelerIds)
    {
        if (!await UnitOfWork.TourGroups.AnyAsync(e => e.Id == tourGroupId))
            return Error.NotFound();

        var records = travelerIds.Select(travelerId => new TravelerInTour
        {
            TravelerId = travelerId,
            TourGroupId = tourGroupId
        });

        // Check all traveler ids
        var existTravelers = await UnitOfWork.Travelers
            .Query()
            .Where(e => travelerIds.Contains(e.Id))
            .Select(e => e.Id).ToListAsync();

        var nonExistTravelers = travelerIds.Except(existTravelers).ToList();

        if (nonExistTravelers.Count != 0)
            return Error.NotFound(nonExistTravelers.Select(id => id.ToString()).ToArray());

        UnitOfWork.TravelersInTours.AddRange(records);

        await UnitOfWork.SaveChangesAsync();
        return Result.Success();
    }

    public async Task<Result> RemoveTravelers(Guid tourGroupId, List<Guid> travelerIds)
    {
        if (!await UnitOfWork.TourGroups.AnyAsync(e => e.Id == tourGroupId))
            return Error.NotFound();

        var records = await UnitOfWork.TravelersInTours
            .Query()
            .Where(e => e.TourGroupId == tourGroupId && travelerIds.Contains(e.TravelerId))
            .ToListAsync();

        UnitOfWork.TravelersInTours.RemoveRange(records);
        await UnitOfWork.SaveChangesAsync();

        return Result.Success();
    }

    public async Task<Result<List<Guid>>> ListTravelers(Guid tourGroupId)
    {
        if (!await UnitOfWork.TourGroups.AnyAsync(e => e.Id == tourGroupId))
            return Error.NotFound();

        var travelerIds = await UnitOfWork.TravelersInTours
            .Query()
            .Where(e => e.TourGroupId == tourGroupId)
            .Select(e => e.TravelerId)
            .ToListAsync();

        return travelerIds;
    }
}