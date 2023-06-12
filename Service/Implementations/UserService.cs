using Data.EFCore;
using Data.Entities;
using Data.Enums;
using Mapster;
using Microsoft.EntityFrameworkCore;
using Service.Commons.Mapping;
using Service.Commons.QueryExtensions;
using Service.Interfaces;
using Service.Models.Admin;
using Service.Models.Staff;
using Service.Models.TourGuide;
using Service.Models.Traveler;
using Service.Models.User;
using Shared.Helpers;
using Shared.ResultExtensions;

namespace Service.Implementations;

public class UserService : BaseService, IUserService
{
    private readonly ICloudStorageService _cloudStorageService;

    public UserService(UnitOfWork unitOfWork, ICloudStorageService cloudStorageService)
        : base(unitOfWork)
    {
        _cloudStorageService = cloudStorageService;
    }

    public async Task<Result<UserViewModel>> Create(UserCreateModel model)
    {
        model.Password = AuthHelper.HashPassword(model.Password);
        User user;
        switch (model.Role)
        {
            case UserRole.Admin:
                user = UnitOfWork.Admins.Add(model.Adapt<Admin>());
                break;
            case UserRole.TourGuide:
                user = UnitOfWork.TourGuides.Add(model.Adapt<TourGuide>());
                break;
            case UserRole.Traveler:
                user = UnitOfWork.Travelers.Add(model.Adapt<Traveler>());
                break;
            case UserRole.Staff:
                user = UnitOfWork.Staffs.Add(model.Adapt<Staff>());
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        await UnitOfWork.SaveChangesAsync();
        return user.Adapt<UserViewModel>();
    }

    public async Task<Result<UserViewModel>> Update(Guid id, UserUpdateModel model)
    {
        var user = await UnitOfWork.Users.FindAsync(id);
        if (user is null) return Error.NotFound();

        model.AdaptIgnoreNull(user);
        UnitOfWork.Users.Update(user);
        await UnitOfWork.SaveChangesAsync();

        var view = user.Adapt<UserViewModel>();
        view.AvatarUrl = _cloudStorageService.GetMediaLink(user.AvatarId);
        return view;
    }

    public async Task<Result<PaginationModel<UserViewModel>>> Filter(UserFilterModel model)
    {
        var query = UnitOfWork.Users.Query();

        if (model.Phone != null) query = query.Where(e => e.Phone.Contains(model.Phone));
        if (model.Email != null) query = query.Where(e => e.Email != null && e.Email.Contains(model.Email));
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
            view.AvatarUrl = _cloudStorageService.GetMediaLink(user.AvatarId);
            return view;
        });
    }

    public async Task<Result<UserViewModel>> GetProfile(Guid id)
    {
        var user = await UnitOfWork.Users.FindAsync(id);
        if (user is null) return Error.NotFound();

        UserViewModel view = user.Role switch
        {
            UserRole.Traveler => (await UnitOfWork.Travelers.FindAsync(id))!.Adapt<TravelerViewModel>(),
            UserRole.TourGuide => (await UnitOfWork.TourGuides.FindAsync(id))!.Adapt<TourGuideViewModel>(),
            UserRole.Admin => (await UnitOfWork.Admins.FindAsync(id))!.Adapt<AdminViewModel>(),
            UserRole.Staff => (await UnitOfWork.Staffs.FindAsync(id))!.Adapt<StaffViewModel>(),
            _ => throw new ArgumentOutOfRangeException()
        };

        view.AvatarUrl = _cloudStorageService.GetMediaLink(user.AvatarId);

        return view;
    }

    public async Task<Result<UserViewModel>> UpdateProfile(Guid id, ProfileUpdateModel model)
    {
        var user = await UnitOfWork.Users.FindAsync(id);
        if (user is null) return Error.NotFound();

        UserViewModel view;
        switch (user.Role)
        {
            case UserRole.Admin:
                var admin = (await UnitOfWork.Admins.FindAsync(user.Id))!;
                UnitOfWork.Admins.Update(model.AdaptIgnoreNull(admin));
                view = admin.Adapt<AdminViewModel>();
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
            case UserRole.Staff:
                var staff = (await UnitOfWork.Staffs.FindAsync(user.Id))!;
                UnitOfWork.Staffs.Update(model.AdaptIgnoreNull(staff));
                view = staff.Adapt<StaffViewModel>();
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        await UnitOfWork.SaveChangesAsync();

        // Return
        view.AvatarUrl = _cloudStorageService.GetMediaLink(user.AvatarId);

        return view;
    }

    public async Task<Result<UserViewModel>> AdminGetUserById(Guid id)
    {
        var entity = await UnitOfWork.Users.Query()
            .Where(e => e.Id == id)
            .FirstOrDefaultAsync();

        if (entity is null) return Error.NotFound();

        // Result
        var viewModel = entity.Adapt<UserViewModel>();
        viewModel.AvatarUrl = _cloudStorageService.GetMediaLink(entity.AvatarId);

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
}