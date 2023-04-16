using Data.EFCore;
using Data.EFCore.Repositories;
using Data.Entities;
using Data.Enums;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Service.Interfaces;
using Service.Models.Account;
using Service.Models.Attachment;
using Shared.ResultExtensions;

namespace Service.Implementations;

public class AccountService : BaseService, IAccountService
{
    private readonly ICloudStorageService _cloudStorageService;
    private readonly IAttachmentService _attachmentService;
    private readonly ILogger<AccountService> _logger;
    private readonly IMapper _mapper;

    //
    private readonly IRepository<Account> _accountRepo;
    private readonly IRepository<Traveler> _travelerRepo;
    private readonly IRepository<TourGuide> _tourGuideRepo;
    private readonly IRepository<Manager> _managerRepo;

    public AccountService(IUnitOfWork unitOfWork, ILogger<AccountService> logger,
        ICloudStorageService cloudStorageService, IMapper mapper, IAttachmentService attachmentService) : base(
        unitOfWork)
    {
        _logger = logger;
        _cloudStorageService = cloudStorageService;
        _mapper = mapper;
        _attachmentService = attachmentService;
        //
        _accountRepo = unitOfWork.Repo<Account>();
        _travelerRepo = unitOfWork.Repo<Traveler>();
        _tourGuideRepo = unitOfWork.Repo<TourGuide>();
        _managerRepo = unitOfWork.Repo<Manager>();
    }

    public async Task<Result<AvatarViewModel>> GetAvatar(Guid id)
    {
        var attachmentId = await _accountRepo
            .Query()
            .Where(e => e.Id == id)
            .Select(e => e.AttachmentId)
            .FirstOrDefaultAsync();

        if (attachmentId is null) return Error.NotFound();

        return new AvatarViewModel(
            attachmentId.Value,
            _cloudStorageService.GetMediaLink(attachmentId.Value)
        );
    }

    public async Task<Result<ProfileViewModel>> GetProfile(Guid id, AccountRole role)
    {
        ProfileViewModel viewModel;
        switch (role)
        {
            case AccountRole.Traveler:
                var traveler = await _travelerRepo.FindAsync(id);
                if (traveler is null) return Error.Unexpected();
                viewModel = _mapper.Map<ProfileViewModel>(traveler);
                break;
            case AccountRole.TourGuide:
                var tourGuide = await _tourGuideRepo.FindAsync(id);
                if (tourGuide is null) return Error.Unexpected();
                viewModel = _mapper.Map<ProfileViewModel>(tourGuide);
                break;
            case AccountRole.Manager:
                var manager = await _managerRepo.FindAsync(id);
                if (manager is null) return Error.Unexpected();
                viewModel = _mapper.Map<ProfileViewModel>(manager);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        return viewModel;
    }

    public async Task<Result<AttachmentViewModel>> UpdateAvatar(Guid id, string contentType, Stream stream)
    {
        await using var transaction = UnitOfWork.BeginTransaction();

        try
        {
            // Get Account
            var account = await _accountRepo
                .Query()
                .Where(e => e.Id == id)
                .Select(e => new Account()
                {
                    Id = id,
                    AttachmentId = e.AttachmentId
                })
                .FirstOrDefaultAsync();

            if (account is null) return Error.Unexpected();

            var oldAttachmentId = account.AttachmentId;

            // Create new Attachment
            var createAttachmentResult = await _attachmentService.Create(contentType, stream);
            if (!createAttachmentResult.IsSuccess)
            {
                await transaction.RollbackAsync();
                return Error.Unexpected();
            }

            UnitOfWork.Attach(account);

            account.AttachmentId = createAttachmentResult.Value.Id;
            await UnitOfWork.SaveChangesAsync();

            // Delete old attachment
            if (oldAttachmentId is not null)
            {
                var deleteAttachmentResult = await _attachmentService.Delete(oldAttachmentId.Value);
                if (!deleteAttachmentResult.IsSuccess)
                    _logger.LogError("Delete attachment failed: {Id}", oldAttachmentId.Value);
            }

            await transaction.CommitAsync();
            return createAttachmentResult.Value;
        }
        catch (Exception e)
        {
            _logger.LogWarning(e, "{Message}", e.Message);
            await transaction.RollbackAsync();
            return Error.Unexpected(e.Message);
        }
    }
}