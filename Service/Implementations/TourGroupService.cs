using Data.EFCore;
using Data.EFCore.Repositories;
using Data.Entities;
using Mapster;
using Microsoft.EntityFrameworkCore;
using Service.Interfaces;
using Service.Models.TourGroup;
using Shared.ResultExtensions;

namespace Service.Implementations;

public class TourGroupService : BaseService, ITourGroupService
{
    private readonly IRepository<Tour> _tourRepo;
    private readonly IRepository<Traveler> _travelerRepo;
    private readonly IRepository<TourGroup> _tourGroupRepo;
    private readonly IRepository<TourGuide> _tourGuideRepo;
    private readonly IRepository<TravelerInTourGroup> _travelerInTourGroupRepo;

    public TourGroupService(IUnitOfWork unitOfWork) : base(unitOfWork)
    {
        _tourRepo = unitOfWork.Repo<Tour>();
        _travelerRepo = unitOfWork.Repo<Traveler>();
        _tourGroupRepo = unitOfWork.Repo<TourGroup>();
        _tourGuideRepo = unitOfWork.Repo<TourGuide>();
        _travelerInTourGroupRepo = unitOfWork.Repo<TravelerInTourGroup>();
    }

    public async Task<Result<TourGroupViewModel>> Create(TourGroupCreateModel model)
    {
        var tour = await _tourRepo.FindAsync(model.TourId);
        if (tour is null) return Error.NotFound();

        var group = _tourGroupRepo.Add(new TourGroup
        {
            TourId = tour.Id,
            GroupName = model.GroupName
        });

        await UnitOfWork.SaveChangesAsync();

        return group.Adapt<TourGroupViewModel>();
    }

    public async Task<Result<TourGroupViewModel>> Update(Guid groupId, TourGroupUpdateModel model)
    {
        var group = await _tourGroupRepo.FindAsync(groupId);
        if (group is null) return Error.NotFound("Tour group not found.");

        if (model.GroupName != null) group.GroupName = model.GroupName;

        if (model.TourGuideId != null)
        {
            var tourGuide = await _tourGuideRepo.FindAsync(model.TourGuideId);
            if (tourGuide is null) return Error.NotFound("Tour guide not found.");
            group.TourGuide = tourGuide;
        }

        _tourGroupRepo.Update(group);

        await UnitOfWork.SaveChangesAsync();

        return group.Adapt<TourGroupViewModel>();
    }

    public async Task<Result> Delete(Guid groupId)
    {
        var group = await _tourGroupRepo.FindAsync(groupId);
        if (group is null) return Error.NotFound();

        _tourGroupRepo.Remove(group);
        await UnitOfWork.SaveChangesAsync();
        return Result.Success();
    }

    public async Task<Result<List<TourGroupViewModel>>> ListGroupsByTour(Guid tourId)
    {
        if (!await _tourRepo.AnyAsync(e => e.Id == tourId)) return Error.NotFound();

        var tourGroups = await _tourGroupRepo
            .Query()
            .Where(e => e.TourId == tourId)
            .ToListAsync();

        return tourGroups.Adapt<List<TourGroupViewModel>>();
    }

    public async Task<Result> AddTravelers(Guid tourGroupId, ICollection<Guid> travelerIds)
    {
        if (!await _tourGroupRepo.AnyAsync(e => e.Id == tourGroupId))
            return Error.NotFound();

        var records = travelerIds.Select(travelerId => new TravelerInTourGroup
        {
            TravelerId = travelerId,
            TourGroupId = tourGroupId
        });

        // Check all traveler ids
        var existTravelers = await _travelerRepo
            .Query()
            .Where(e => travelerIds.Contains(e.Id))
            .Select(e => e.Id).ToListAsync();

        var nonExistTravelers = travelerIds.Except(existTravelers).ToList();

        if (nonExistTravelers.Count != 0)
            return Error.NotFound(nonExistTravelers.Select(id => id.ToString()).ToArray());

        _travelerInTourGroupRepo.AddRange(records);

        await UnitOfWork.SaveChangesAsync();
        return Result.Success();
    }

    public async Task<Result> RemoveTravelers(Guid tourGroupId, List<Guid> travelerIds)
    {
        if (!await _tourGroupRepo.AnyAsync(e => e.Id == tourGroupId))
            return Error.NotFound();

        var records = await _travelerInTourGroupRepo
            .Query()
            .Where(e => e.TourGroupId == tourGroupId && travelerIds.Contains(e.TravelerId))
            .ToListAsync();

        _travelerInTourGroupRepo.RemoveRange(records);
        await UnitOfWork.SaveChangesAsync();

        return Result.Success();
    }

    public async Task<Result<List<Guid>>> ListTravelers(Guid tourGroupId)
    {
        if (!await _tourGroupRepo.AnyAsync(e => e.Id == tourGroupId))
            return Error.NotFound();

        var travelerIds = await _travelerInTourGroupRepo
            .Query()
            .Where(e => e.TourGroupId == tourGroupId)
            .Select(e => e.TravelerId)
            .ToListAsync();

        return travelerIds;
    }
}