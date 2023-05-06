using Data.EFCore;
using Data.Enums;
using Mapster;
using MapsterMapper;
using Service.Commons;
using Service.Interfaces;
using Service.Models.Account;
using Service.Models.Manager;
using Service.Models.TourGuide;
using Service.Models.Traveler;
using Shared.ResultExtensions;

namespace Service.Implementations;

public class AccountService : BaseService, IAccountService
{
    private readonly ICloudStorageService _cloudStorageService;
    private readonly IMapper _mapper;

    public AccountService(UnitOfWork unitOfWork, ICloudStorageService cloudStorageService, IMapper mapper)
        : base(unitOfWork)
    {
        _cloudStorageService = cloudStorageService;
        _mapper = mapper;
    }

    public async Task<Result<AccountViewModel>> GetProfile(Guid id, AccountRole role)
    {
        var account = await UnitOfWork.Accounts.FindAsync(id);
        if (account is null) return Error.NotFound();

        var view = role switch
        {
            AccountRole.Traveler => _mapper.Map<TravelerViewModel>((await UnitOfWork.Travelers.FindAsync(id))!),
            AccountRole.TourGuide => _mapper.Map<AccountViewModel>((await UnitOfWork.TourGuides.FindAsync(id))!),
            AccountRole.Manager => _mapper.Map<ManagerViewModel>((await UnitOfWork.Managers.FindAsync(id))!),
            _ => throw new ArgumentOutOfRangeException()
        };

        if (account.AvatarId != null)
            view.AvatarUrl = _cloudStorageService.GetMediaLink(account.AvatarId.Value);

        return view;
    }

    public async Task<Result<AccountViewModel>> UpdateProfile(Guid id, ProfileUpdateModel model)
    {
        var account = await UnitOfWork.Accounts.FindAsync(id);
        if (account is null) return Error.NotFound();

        AccountViewModel view;
        switch (account.Role)
        {
            case AccountRole.Manager:
                var manager = (await UnitOfWork.Managers.FindAsync(account.Id))!;
                UnitOfWork.Managers.Update(model.AdaptIgnoreNull(manager));
                view = manager.Adapt<ManagerViewModel>();
                break;
            case AccountRole.TourGuide:
                var tourGuide = (await UnitOfWork.TourGuides.FindAsync(account.Id))!;
                UnitOfWork.TourGuides.Update(model.AdaptIgnoreNull(tourGuide));
                view = tourGuide.Adapt<TourGuideViewModel>();
                break;
            case AccountRole.Traveler:
                var traveler = (await UnitOfWork.Travelers.FindAsync(account.Id))!;
                UnitOfWork.Travelers.Update(model.AdaptIgnoreNull(traveler));
                view = traveler.Adapt<TravelerViewModel>();
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        // Return
        if (account.AvatarId != null)
            view.AvatarUrl = _cloudStorageService.GetMediaLink(account.AvatarId.Value);

        return view;
    }
}