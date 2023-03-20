using Data.EFCore;
using Data.Entities;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Service.Interfaces;
using Service.Models.Account;
using Service.Models.Attachment;
using Shared.Enums;
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
        var attachmentId = await _unitOfWork.Repo<Account>()
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

    public async Task<Result<ProfileViewModel>> GetProfile(Guid id, UserRole role)
    {
        ProfileViewModel viewModel;
        switch (role)
        {
            case UserRole.Traveler:
                var traveler = await _unitOfWork.Repo<Traveler>().FirstOrDefaultAsync(e => e.Id == id);
                if (traveler is null) return Error.Unexpected();
                viewModel = _mapper.Map<ProfileViewModel>(traveler);
                break;
            case UserRole.TourGuide:
                var tourGuide = await _unitOfWork.Repo<TourGuide>().FirstOrDefaultAsync(e => e.Id == id);
                if (tourGuide is null) return Error.Unexpected();
                viewModel = _mapper.Map<ProfileViewModel>(tourGuide);
                break;
            case UserRole.Manager:
                var manager = await _unitOfWork.Repo<Manager>().FirstOrDefaultAsync(e => e.Id == id);
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
        await using var transaction = _unitOfWork.BeginTransaction();

        try
        {
            var account = _unitOfWork.Repo<Account>()
                .TrackingQuery()
                .Include(e => e.Attachment)
                .FirstOrDefault(e => e.Id == id);

            if (account is null) return Error.Unexpected();

            var oldAttachment = account.Attachment;

            var newAttachment = _unitOfWork.Repo<Attachment>().Add(new Attachment
            {
                ContentType = contentType,
                CreatedAt = DateTimeHelper.VnNow()
            });

            account.Attachment = newAttachment;
            await _unitOfWork.SaveChangesAsync();

            if (oldAttachment is not null)
            {
                _unitOfWork.Repo<Attachment>().Remove(oldAttachment);
                await _unitOfWork.SaveChangesAsync();
                var result = await _cloudStorageService.Delete(oldAttachment.Id);
                if (!result.IsSuccess)
                {
                    await transaction.RollbackAsync();
                    return Error.Unexpected();
                }
            }

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