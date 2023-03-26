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
        var tour = await _unitOfWork.Repo<Tour>().FirstOrDefaultAsync(e => e.Id == model.TourId);
        if (tour is null) return Error.NotFound();

        var group = _unitOfWork.Repo<TourGroup>().Add(new TourGroup()
        {
            Tour = tour,
            GroupName = model.GroupName
        });

        await _unitOfWork.SaveChangesAsync();

        return group.Adapt<TourGroupViewModel>();
    }

    public async Task<Result<TourGroupViewModel>> Update(Guid groupId, TourGroupUpdateModel model)
    {
        var group = await _unitOfWork.Repo<TourGroup>().FirstOrDefaultAsync(e => e.Id == groupId);
        if (group is null) return Error.NotFound("Tour group not found.");

        if (model.GroupName != null) group.GroupName = model.GroupName;

        if (model.TourGuide != null)
        {
            var tourGuide = await _unitOfWork.Repo<TourGuide>().FirstOrDefaultAsync(e => e.Id == model.TourGuide);
            if (tourGuide is null) return Error.NotFound("Tour guide not found.");
            group.TourGuide = tourGuide;
        }

        await _unitOfWork.SaveChangesAsync();

        return group.Adapt<TourGroupViewModel>();
    }

    public async Task<Result> Delete(Guid groupId)
    {
        var group = await _unitOfWork.Repo<TourGroup>().FirstOrDefaultAsync(e => e.Id == groupId);
        if (group is null) return Error.NotFound();

        _unitOfWork.Repo<TourGroup>().Remove(group);
        await _unitOfWork.SaveChangesAsync();
        return Result.Success();
    }

    public async Task<Result<List<TourGroupViewModel>>> ListGroupsByTour(Guid tourId)
    {
        if (!await _unitOfWork.Repo<Tour>().AnyAsync(e => e.Id == tourId)) return Error.NotFound();
        var tourGroups = await _unitOfWork.Repo<TourGroup>().Query().Where(e => e.TourId == tourId).ToListAsync();
        return tourGroups.Adapt<List<TourGroupViewModel>>();
    }

    public async Task<Result> AddTravelers(Guid tourGroupId, ICollection<Guid> travelerIds)
    {
        if (!await _unitOfWork.Repo<TourGroup>().AnyAsync(e => e.Id == tourGroupId))
            return Error.NotFound();

        var records = travelerIds.Select(travelerId => new TravelerInTourGroup()
        {
            TravelerId = travelerId,
            TourGroupId = tourGroupId
        });

        // Check all traveler ids
        var existTravelers = await _unitOfWork.Repo<Traveler>()
            .Query()
            .Where(e => travelerIds.Contains(e.Id))
            .Select(e => e.Id).ToListAsync();

        var nonExistTravelers = travelerIds.Except(existTravelers).ToList();

        if (nonExistTravelers.Count != 0)
            return Error.NotFound(nonExistTravelers.Select(id => id.ToString()).ToArray());

        _unitOfWork.Repo<TravelerInTourGroup>().AddRange(records);

        await _unitOfWork.SaveChangesAsync();
        return Result.Success();
    }

    public async Task<Result> RemoveTravelers(Guid tourGroupId, List<Guid> travelerIds)
    {
        if (!await _unitOfWork.Repo<TourGroup>().AnyAsync(e => e.Id == tourGroupId))
            return Error.NotFound();

        var records = await _unitOfWork.Repo<TravelerInTourGroup>()
            .Query()
            .Where(e => e.TourGroupId == tourGroupId && travelerIds.Contains(e.TravelerId))
            .ToListAsync();

        _unitOfWork.Repo<TravelerInTourGroup>().RemoveRange(records);
        await _unitOfWork.SaveChangesAsync();

        return Result.Success();
    }

    public async Task<Result<List<Guid>>> ListTravelers(Guid tourGroupId)
    {
        if (!await _unitOfWork.Repo<TourGroup>().AnyAsync(e => e.Id == tourGroupId))
            return Error.NotFound();

        var travelerIds = await _unitOfWork.Repo<TravelerInTourGroup>()
            .Query()
            .Where(e => e.TourGroupId == tourGroupId)
            .Select(e => e.TravelerId)
            .ToListAsync();

        return travelerIds;
    }
}