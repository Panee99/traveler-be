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
    public TourGroupService(IUnitOfWork unitOfWork) : base(unitOfWork)
    {
    }

    public async Task<Result<TourGroupViewModel>> Create(TourGroupCreateModel model)
    {
        var tour = await UnitOfWork.Repo<Tour>().FirstOrDefaultAsync(e => e.Id == model.TourId);
        if (tour is null) return Error.NotFound();

        var group = UnitOfWork.Repo<TourGroup>().Add(new TourGroup
        {
            TourId = tour.Id,
            GroupName = model.GroupName
        });

        await UnitOfWork.SaveChangesAsync();

        return group.Adapt<TourGroupViewModel>();
    }

    public async Task<Result<TourGroupViewModel>> Update(Guid groupId, TourGroupUpdateModel model)
    {
        var group = await UnitOfWork.Repo<TourGroup>().FirstOrDefaultAsync(e => e.Id == groupId);
        if (group is null) return Error.NotFound("Tour group not found.");

        if (model.GroupName != null) group.GroupName = model.GroupName;

        if (model.TourGuide != null)
        {
            var tourGuide = await UnitOfWork.Repo<TourGuide>().FirstOrDefaultAsync(e => e.Id == model.TourGuide);
            if (tourGuide is null) return Error.NotFound("Tour guide not found.");
            group.TourGuide = tourGuide;
        }

        UnitOfWork.Repo<TourGroup>().Update(group);

        await UnitOfWork.SaveChangesAsync();

        return group.Adapt<TourGroupViewModel>();
    }

    public async Task<Result> Delete(Guid groupId)
    {
        var group = await UnitOfWork.Repo<TourGroup>().FirstOrDefaultAsync(e => e.Id == groupId);
        if (group is null) return Error.NotFound();

        UnitOfWork.Repo<TourGroup>().Remove(group);
        await UnitOfWork.SaveChangesAsync();
        return Result.Success();
    }

    public async Task<Result<List<TourGroupViewModel>>> ListGroupsByTour(Guid tourId)
    {
        if (!await UnitOfWork.Repo<Tour>().AnyAsync(e => e.Id == tourId)) return Error.NotFound();

        var tourGroups = await UnitOfWork.Repo<TourGroup>()
            .Query()
            .Where(e => e.TourId == tourId)
            .ToListAsync();

        return tourGroups.Adapt<List<TourGroupViewModel>>();
    }

    public async Task<Result> AddTravelers(Guid tourGroupId, ICollection<Guid> travelerIds)
    {
        if (!await UnitOfWork.Repo<TourGroup>().AnyAsync(e => e.Id == tourGroupId))
            return Error.NotFound();

        var records = travelerIds.Select(travelerId => new TravelerInTourGroup
        {
            TravelerId = travelerId,
            TourGroupId = tourGroupId
        });

        // Check all traveler ids
        var existTravelers = await UnitOfWork.Repo<Traveler>()
            .Query()
            .Where(e => travelerIds.Contains(e.Id))
            .Select(e => e.Id).ToListAsync();

        var nonExistTravelers = travelerIds.Except(existTravelers).ToList();

        if (nonExistTravelers.Count != 0)
            return Error.NotFound(nonExistTravelers.Select(id => id.ToString()).ToArray());

        UnitOfWork.Repo<TravelerInTourGroup>().AddRange(records);

        await UnitOfWork.SaveChangesAsync();
        return Result.Success();
    }

    public async Task<Result> RemoveTravelers(Guid tourGroupId, List<Guid> travelerIds)
    {
        if (!await UnitOfWork.Repo<TourGroup>().AnyAsync(e => e.Id == tourGroupId))
            return Error.NotFound();

        var records = await UnitOfWork.Repo<TravelerInTourGroup>()
            .Query()
            .Where(e => e.TourGroupId == tourGroupId && travelerIds.Contains(e.TravelerId))
            .ToListAsync();

        UnitOfWork.Repo<TravelerInTourGroup>().RemoveRange(records);
        await UnitOfWork.SaveChangesAsync();

        return Result.Success();
    }

    public async Task<Result<List<Guid>>> ListTravelers(Guid tourGroupId)
    {
        if (!await UnitOfWork.Repo<TourGroup>().AnyAsync(e => e.Id == tourGroupId))
            return Error.NotFound();

        var travelerIds = await UnitOfWork.Repo<TravelerInTourGroup>()
            .Query()
            .Where(e => e.TourGroupId == tourGroupId)
            .Select(e => e.TravelerId)
            .ToListAsync();

        return travelerIds;
    }
}