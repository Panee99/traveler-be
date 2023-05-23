using Data.EFCore;
using Data.Entities;
using Data.Enums;
using Mapster;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using Service.Commons;
using Service.Commons.Pagination;
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
    private readonly IMapper _mapper;

    public UserService(UnitOfWork unitOfWork, ICloudStorageService cloudStorageService, IMapper mapper)
        : base(unitOfWork)
    {
        _cloudStorageService = cloudStorageService;
        _mapper = mapper;
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
        if (user.AvatarId != null) view.AvatarUrl = _cloudStorageService.GetMediaLink(user.AvatarId.Value);
        return view;
    }

    public async Task<Result<PaginationModel<UserViewModel>>> Filter(UserFilterModel model)
    {
        var query = UnitOfWork.Users.Query();

        if (model.Phone != null) query = query.Where(e => e.Phone.Contains(model.Phone));
        if (model.Email != null) query = query.Where(e => e.Email != null && e.Email.Contains(model.Email));
        if (model.Role != null) query = query.Where(e => e.Role == model.Role);
        if (model.Status != null) query = query.Where(e => e.Status == model.Status);

        var result = await query.Paging(model.Page, model.Size);

        return result.Map(user =>
        {
            var view = user.Adapt<UserViewModel>();
            if (user.AvatarId != null)
                view.AvatarUrl = _cloudStorageService.GetMediaLink(user.AvatarId.Value);
            return view;
        });
    }

    public async Task<Result<UserViewModel>> GetProfile(Guid id)
    {
        var user = await UnitOfWork.Users.FindAsync(id);
        if (user is null) return Error.NotFound();

        UserViewModel view = user.Role switch
        {
            UserRole.Traveler => _mapper.Map<TravelerViewModel>((await UnitOfWork.Travelers.FindAsync(id))!),
            UserRole.TourGuide => _mapper.Map<TourGuideViewModel>((await UnitOfWork.TourGuides.FindAsync(id))!),
            UserRole.Admin => _mapper.Map<AdminViewModel>((await UnitOfWork.Admins.FindAsync(id))!),
            UserRole.Staff => _mapper.Map<StaffViewModel>((await UnitOfWork.Staffs.FindAsync(id))!),
            _ => throw new ArgumentOutOfRangeException()
        };

        if (user.AvatarId != null)
            view.AvatarUrl = _cloudStorageService.GetMediaLink(user.AvatarId.Value);

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
        if (user.AvatarId != null)
            view.AvatarUrl = _cloudStorageService.GetMediaLink(user.AvatarId.Value);

        return view;
    }

    public async Task<Result<UserViewModel>> AdminGetUserById(Guid id)
    {
        var entity = await UnitOfWork.Users.Query()
            .Where(e => e.Id == id)
            .FirstOrDefaultAsync();

        if (entity is null) return Error.NotFound();

        // Result
        var viewModel = _mapper.Map<UserViewModel>(entity);
        viewModel.AvatarUrl = entity.AvatarId != null ? _cloudStorageService.GetMediaLink(entity.AvatarId.Value) : null;

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