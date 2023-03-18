using Data;
using Data.Entities;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Service.Interfaces;
using Service.Models.Attachment;
using Service.Results;
using Shared;

namespace Service.Implementations;

public class AccountService : BaseService, IAccountService
{
    private readonly ILogger<AccountService> _logger;
    private readonly ICloudStorageService _cloudStorageService;
    private readonly IMapper _mapper;

    public AccountService(IUnitOfWork unitOfWork, ILogger<AccountService> logger,
        ICloudStorageService cloudStorageService, IMapper mapper) : base(unitOfWork)
    {
        _logger = logger;
        _cloudStorageService = cloudStorageService;
        _mapper = mapper;
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

            var newAttachment = _unitOfWork.Repo<Attachment>().Add(new Attachment()
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
            
            return new AttachmentViewModel()
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