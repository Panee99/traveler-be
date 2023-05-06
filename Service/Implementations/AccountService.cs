using Data.EFCore;
using Data.EFCore.Repositories;
using Data.Entities;
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

    //
    private readonly IRepository<Account> _accountRepo;
    private readonly IRepository<Traveler> _travelerRepo;
    private readonly IRepository<TourGuide> _tourGuideRepo;
    private readonly IRepository<Manager> _managerRepo;

    public AccountService(IUnitOfWork unitOfWork, ICloudStorageService cloudStorageService, IMapper mapper) : base(
        unitOfWork)
    {
        _cloudStorageService = cloudStorageService;
        _mapper = mapper;
        //
        _accountRepo = unitOfWork.Repo<Account>();
        _travelerRepo = unitOfWork.Repo<Traveler>();
        _tourGuideRepo = unitOfWork.Repo<TourGuide>();
        _managerRepo = unitOfWork.Repo<Manager>();
    }

    public async Task<Result<AccountViewModel>> GetProfile(Guid id, AccountRole role)
    {
        var account = await _accountRepo.FindAsync(id);
        if (account is null) return Error.NotFound();

        var view = role switch
        {
            AccountRole.Traveler => _mapper.Map<TravelerViewModel>((await _travelerRepo.FindAsync(id))!),
            AccountRole.TourGuide => _mapper.Map<AccountViewModel>((await _tourGuideRepo.FindAsync(id))!),
            AccountRole.Manager => _mapper.Map<ManagerViewModel>((await _managerRepo.FindAsync(id))!),
            _ => throw new ArgumentOutOfRangeException()
        };

        if (account.AvatarId != null)
            view.AvatarUrl = _cloudStorageService.GetMediaLink(account.AvatarId.Value);

        return view;
    }

    public async Task<Result<AccountViewModel>> UpdateProfile(Guid id, ProfileUpdateModel model)
    {
        var account = await _accountRepo.FindAsync(id);
        if (account is null) return Error.NotFound();

        AccountViewModel view;
        switch (account.Role)
        {
            case AccountRole.Manager:
                var manager = (await _managerRepo.FindAsync(account.Id))!;
                _managerRepo.Update(model.AdaptIgnoreNull(manager));
                view = manager.Adapt<ManagerViewModel>();
                break;
            case AccountRole.TourGuide:
                var tourGuide = (await _tourGuideRepo.FindAsync(account.Id))!;
                _tourGuideRepo.Update(model.AdaptIgnoreNull(tourGuide));
                view = tourGuide.Adapt<TourGuideViewModel>();
                break;
            case AccountRole.Traveler:
                var traveler = (await _travelerRepo.FindAsync(account.Id))!;
                _travelerRepo.Update(model.AdaptIgnoreNull(traveler));
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