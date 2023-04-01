using Data.EFCore;
using Data.Entities;
using Microsoft.Extensions.Logging;
using Service.Interfaces;
using Shared.ResultExtensions;

namespace Service.Implementations;

public class AttachmentService : BaseService, IAttachmentService
{
    private readonly ICloudStorageService _cloudStorageService;
    private readonly ILogger<AttachmentService> _logger;

    public AttachmentService(IUnitOfWork unitOfWork, ICloudStorageService cloudStorageService,
        ILogger<AttachmentService> logger) : base(unitOfWork)
    {
        _cloudStorageService = cloudStorageService;
        _logger = logger;
    }

    public async Task<Result> Delete(Guid id)
    {
        try
        {
            UnitOfWork.Repo<Attachment>().Remove(new Attachment() { Id = id });
            await UnitOfWork.SaveChangesAsync();
        }
        catch (Exception e)
        {
            _logger.LogWarning(e, "{Message}", e.Message);
            return Error.Unexpected();
        }

        var cloudResult = await _cloudStorageService.Delete(id);
        return cloudResult.IsSuccess ? Result.Success() : Error.Unexpected();
    }
}