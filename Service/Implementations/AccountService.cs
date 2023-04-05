using Data.EFCore;
using Data.Entities;
using Data.Enums;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Service.Interfaces;
using Service.Models.Account;
using Service.Models.Attachment;
using Shared.Helpers;
using Shared.ResultExtensions;

namespace Service.Implementations;

public class AccountService : BaseService, IAccountService
{
    private readonly ICloudStorageService _cloudStorageService;
    private readonly ILogger<AccountService> _logger;
    private readonly IMapper _mapper;

    public AccountService(IUnitOfWork unitOfWork, ILogger<AccountService> logger,
        ICloudStorageService cloudStorageService, IMapper mapper) : base(unitOfWork)
    {
        _logger = logger;
        _cloudStorageService = cloudStorageService;
        _mapper = mapper;
    }

    public async Task<Result<AvatarViewModel>> GetAvatar(Guid id)
    {
        var attachmentId = await UnitOfWork.Repo<Account>()
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
                var traveler = await UnitOfWork.Repo<Traveler>().FirstOrDefaultAsync(e => e.Id == id);
                if (traveler is null) return Error.Unexpected();
                viewModel = _mapper.Map<ProfileViewModel>(traveler);
                break;
            case AccountRole.TourGuide:
                var tourGuide = await UnitOfWork.Repo<TourGuide>().FirstOrDefaultAsync(e => e.Id == id);
                if (tourGuide is null) return Error.Unexpected();
                viewModel = _mapper.Map<ProfileViewModel>(tourGuide);
                break;
            case AccountRole.Manager:
                var manager = await UnitOfWork.Repo<Manager>().FirstOrDefaultAsync(e => e.Id == id);
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
            // Check if user exist
            var account = await UnitOfWork.Repo<Account>()
                .TrackingQuery()
                .Include(e => e.Attachment)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (account is null) return Error.Unexpected();

            var oldAttachment = account.Attachment;

            // Create new attachment in DB
            var newAttachment = UnitOfWork.Repo<Attachment>().Add(new Attachment
            {
                ContentType = contentType,
                CreatedAt = DateTimeHelper.VnNow()
            });

            account.Attachment = newAttachment;
            await UnitOfWork.SaveChangesAsync();

            // Delete old attachment from DB and Cloud
            if (oldAttachment is not null)
            {
                UnitOfWork.Repo<Attachment>().Remove(oldAttachment);
                await UnitOfWork.SaveChangesAsync();
                var result = await _cloudStorageService.Delete(oldAttachment.Id);
                if (!result.IsSuccess)
                {
                    await transaction.RollbackAsync();
                    return Error.Unexpected();
                }
            }

            // Upload new attachment to Cloud
            var uploadResult = await _cloudStorageService.Upload(newAttachment.Id, contentType, stream);
            if (!uploadResult.IsSuccess)
            {
                await transaction.RollbackAsync();
                return Error.Unexpected();
            }

            await transaction.CommitAsync();

            return new AttachmentViewModel
            {
                Id = newAttachment.Id,
                ContentType = newAttachment.ContentType,
                Url = uploadResult.Value
            };
        }
        catch (Exception e)
        {
            _logger.LogWarning(e, "{Message}", e.Message);
            await transaction.RollbackAsync();
            return Error.Unexpected();
        }
    }
}