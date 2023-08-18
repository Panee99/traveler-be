using System.Text.RegularExpressions;
using Data.EFCore;
using Data.Entities;
using Data.Enums;
using Mapster;
using Microsoft.EntityFrameworkCore;
using Service.Commons.Mapping;
using Service.Commons.QueryExtensions;
using Service.Interfaces;
using Service.Models.Manager;
using Service.Models.TourGroup;
using Service.Models.TourGuide;
using Service.Models.Traveler;
using Service.Models.User;
using Shared.Helpers;
using Shared.ResultExtensions;

namespace Service.Implementations;

public class UserService : BaseService, IUserService
{
    private readonly ICloudStorageService _cloudStorageService;
    private readonly ITourGroupService _tourGroupService;

    public UserService(UnitOfWork unitOfWork, ICloudStorageService cloudStorageService,
        ITourGroupService tourGroupService)
        : base(unitOfWork)
    {
        _cloudStorageService = cloudStorageService;
        _tourGroupService = tourGroupService;
    }

    public async Task<Result<UserViewModel>> Create(UserCreateModel model)
    {
        model.Password = AuthHelper.HashPassword(model.Password);
        User user;
        switch (model.Role)
        {
            case UserRole.Manager:
                user = UnitOfWork.Managers.Add(model.Adapt<Manager>());
                break;
            case UserRole.TourGuide:
                user = UnitOfWork.TourGuides.Add(model.Adapt<TourGuide>());
                break;
            case UserRole.Traveler:
                user = UnitOfWork.Travelers.Add(model.Adapt<Traveler>());
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        await UnitOfWork.SaveChangesAsync();
        return user.Adapt<UserViewModel>();
    }

    public async Task<Result<UserViewModel>> Update(Guid id, UserUpdateModel model)
    {
        var user = await UnitOfWork.Users
            .Query()
            .Where(e => e.Id == id)
            .Include(e => e.Avatar)
            .FirstOrDefaultAsync();

        if (user is null) return Error.NotFound();

        model.AdaptIgnoreNull(user);
        UnitOfWork.Users.Update(user);
        await UnitOfWork.SaveChangesAsync();

        var view = user.Adapt<UserViewModel>();
        view.AvatarUrl = _cloudStorageService.GetMediaLink(user.Avatar?.FileName);
        return view;
    }

    public async Task<Result<PaginationModel<UserViewModel>>> Filter(UserFilterModel model)
    {
        var query = UnitOfWork.Users.Query().Include(e => e.Avatar).AsSplitQuery();

        if (model.Phone != null) query = query.Where(e => e.Phone != null && e.Phone.Contains(model.Phone));
        if (model.Email != null) query = query.Where(e => e.Email.Contains(model.Email));
        if (model.FirstName != null) query = query.Where(e => e.FirstName.Contains(model.FirstName));
        if (model.LastName != null) query = query.Where(e => e.LastName.Contains(model.LastName));
        if (model.Gender != null) query = query.Where(e => e.Gender == model.Gender);
        if (model.Role != null) query = query.Where(e => e.Role == model.Role);
        if (model.Status != null) query = query.Where(e => e.Status == model.Status);

        if (model.OrderBy != null)
            query = query.ApplyOrderBy(model.OrderBy.Property, model.OrderBy.Order);

        var result = await query.Paging(model.Page, model.Size);

        return result.Map(user =>
        {
            var view = user.Adapt<UserViewModel>();
            view.AvatarUrl = _cloudStorageService.GetMediaLink(user.Avatar?.FileName);
            return view;
        });
    }

    public async Task<Result<UserViewModel>> GetProfile(Guid id)
    {
        var user = await UnitOfWork.Users.Query()
            .Where(e => e.Id == id)
            .Include(e => e.Avatar)
            .FirstOrDefaultAsync();

        if (user is null) return Error.NotFound();

        UserViewModel view = user.Role switch
        {
            UserRole.Traveler => (await UnitOfWork.Travelers.FindAsync(id))!.Adapt<TravelerViewModel>(),
            UserRole.TourGuide => (await UnitOfWork.TourGuides.FindAsync(id))!.Adapt<TourGuideViewModel>(),
            UserRole.Manager => (await UnitOfWork.Managers.FindAsync(id))!.Adapt<ManagerViewModel>(),
            _ => throw new ArgumentOutOfRangeException()
        };

        view.AvatarUrl = _cloudStorageService.GetMediaLink(user.Avatar?.FileName);

        return view;
    }

    public async Task<Result<UserViewModel>> UpdateProfile(Guid id, ProfileUpdateModel model)
    {
        var user = await UnitOfWork.Users.Query()
            .Where(e => e.Id == id)
            .Include(e => e.Avatar)
            .FirstOrDefaultAsync();

        if (user is null) return Error.NotFound();

        UserViewModel view;
        switch (user.Role)
        {
            case UserRole.Manager:
                var manager = (await UnitOfWork.Managers.FindAsync(user.Id))!;
                UnitOfWork.Managers.Update(model.AdaptIgnoreNull(manager));
                view = manager.Adapt<ManagerViewModel>();
                break;
            case UserRole.TourGuide:
                var tourGuide = (await UnitOfWork.TourGuides.FindAsync(user.Id))!;
                UnitOfWork.TourGuides.Update(model.AdaptIgnoreNull(tourGuide));
                view = tourGuide.Adapt<TourGuideViewModel>();
                break;
            case UserRole.Traveler:
                var traveler = (await UnitOfWork.Travelers.FindAsync(user.Id))!;
                UnitOfWork.Travelers.Update(model.AdaptIgnoreNull(traveler));
                view = traveler.Adapt<TravelerViewModel>();
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        await UnitOfWork.SaveChangesAsync();

        // Return
        view.AvatarUrl = _cloudStorageService.GetMediaLink(user.Avatar?.FileName);

        return view;
    }

    public async Task<Result<UserViewModel>> AdminGetUserById(Guid id)
    {
        var user = await UnitOfWork.Users.Query()
            .Where(e => e.Id == id)
            .Include(e => e.Avatar)
            .FirstOrDefaultAsync();

        if (user is null) return Error.NotFound();

        // Result
        var viewModel = user.Adapt<UserViewModel>();
        viewModel.AvatarUrl = _cloudStorageService.GetMediaLink(user.Avatar?.FileName);

        return viewModel;
    }

    public async Task<Result> AdminDeleteUserById(Guid id)
    {
        var entity = await UnitOfWork.Users.FindAsync(id);
        if (entity is null) return Error.NotFound();

        UnitOfWork.Users.Remove(entity);
        await UnitOfWork.SaveChangesAsync();

        return Result.Success();
    }

    public async Task<Result> ChangePassword(Guid currentUserId, PasswordUpdateModel model)
    {
        if (!Regex.Match(model.Password, "^[a-zA-Z0-9]{6,20}$").Success)
            return Error.Validation("Invalid. Password length 6-20, characters and numbers only");

        var user = await UnitOfWork.Users.FindAsync(currentUserId);
        if (user is null) return Error.Unexpected();

        user.Password = AuthHelper.HashPassword(model.Password);
        UnitOfWork.Users.Update(user);
        await UnitOfWork.SaveChangesAsync();

        return Result.Success();
    }

    public async Task<Result<TravelInfo>> GetTravelInfo(Guid userId)
    {
        var user = await UnitOfWork.Users.FindAsync(userId);
        if (user is null) return Error.NotFound(DomainErrors.User.NotFound);

        int tourCount;

        switch (user.Role)
        {
            case UserRole.Traveler:
            {
                tourCount = await UnitOfWork.Travelers
                    .Query()
                    .Where(traveler => traveler.Id == userId)
                    .SelectMany(traveler => traveler.TourGroups)
                    // Filter out deleted Tour and Trip
                    .Where(group => group.Trip.DeletedById == null &&
                                    group.Trip.Tour.DeletedById == null)
                    .CountAsync();
                break;
            }
            case UserRole.TourGuide:
            {
                tourCount = await UnitOfWork.TourGuides
                    .Query()
                    .Where(tourGuide => tourGuide.Id == userId)
                    .SelectMany(tourGuide => tourGuide.TourGroups)
                    // Filter out deleted Tour and Trip
                    .Where(group => group.Trip.DeletedById == null &&
                                    group.Trip.Tour.DeletedById == null)
                    .CountAsync();
                break;
            }
            case UserRole.Manager:
                return Error.Authorization(DomainErrors.Auth.NotSupportedRole);
            default:
                throw new ArgumentOutOfRangeException();
        }

        return new TravelInfo()
        {
            TourCount = tourCount
        };
    }

    public async Task<Result<CurrentTourGroupViewModel>> GetCurrentJoinedGroup(Guid userId)
    {
        // Check user exist
        var user = await UnitOfWork.Users.FindAsync(userId);
        if (user is null) return Error.NotFound(DomainErrors.User.NotFound);

        TourGroup? currentGroup;

        switch (user.Role)
        {
            case UserRole.Traveler:
                // Get traveler current joined group
                currentGroup = await UnitOfWork.Travelers
                    .Query()
                    .AsSplitQuery()
                    .Where(traveler => traveler.Id == userId)
                    .SelectMany(traveler => traveler.TourGroups)
                    // Filter out deleted Tour and Trip
                    .Where(group => group.Trip.DeletedById == null &&
                                    group.Trip.Tour.DeletedById == null)
                    // Include tour and trip data
                    .Include(group => group.Trip)
                    .ThenInclude(trip => trip.Tour).ThenInclude(tour => tour.Thumbnail)
                    // Filter out group Status
                    .Where(group => group.Status != TourGroupStatus.Ended &&
                                    group.Status != TourGroupStatus.Canceled)
                    .FirstOrDefaultAsync();
                break;
            case UserRole.TourGuide:
                // Get tour guide current joined group
                currentGroup = await UnitOfWork.TourGuides
                    .Query()
                    .AsSplitQuery()
                    .Where(guide => guide.Id == userId)
                    .SelectMany(guide => guide.TourGroups)
                    // Filter out deleted Tour and Trip
                    .Where(group => group.Trip.DeletedById == null &&
                                    group.Trip.Tour.DeletedById == null)
                    // Include tour and trip data
                    .Include(group => group.Trip)
                    .ThenInclude(trip => trip.Tour).ThenInclude(tour => tour.Thumbnail)
                    // Filter out group Status
                    .Where(group => group.Status != TourGroupStatus.Ended &&
                                    group.Status != TourGroupStatus.Canceled)
                    .FirstOrDefaultAsync();
                break;
            default:
                return Error.Authorization(DomainErrors.Auth.NotSupportedRole);
        }

        if (currentGroup is null) return Error.NotFound(DomainErrors.User.NoCurrentGroup);
        // Map to view model
        var tour = currentGroup.Trip.Tour;
        var view = currentGroup.Adapt<CurrentTourGroupViewModel>();

        view.TotalDays = await UnitOfWork.Schedules
            .Query()
            .Where(s => s.TourId == view.Trip!.TourId)
            .OrderBy(s => s.DayNo)
            .Select(s => s.DayNo)
            .LastOrDefaultAsync();

        view.Trip!.Tour!.ThumbnailUrl = _cloudStorageService.GetMediaLink(tour.Thumbnail?.FileName);
        view.TravelerCount = await _tourGroupService.CountTravelers(view.Id);

        return view;
    }

    public async Task<Result<ICollection<UserViewModel>>> FetchUsersInfo(ICollection<Guid> ids)
    {
        return (await UnitOfWork.Users.Query().Where(x => ids.Contains(x.Id)).ToListAsync())
            .Adapt<List<UserViewModel>>();
    }
}