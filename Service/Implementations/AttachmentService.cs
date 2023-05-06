using Data.EFCore;
using Data.Entities;
using Service.Interfaces;
using Service.Models.Attachment;
using Shared.ResultExtensions;

namespace Service.Implementations;

public class AttachmentService : BaseService, IAttachmentService
{
    private readonly ICloudStorageService _cloudStorageService;

    public AttachmentService(UnitOfWork unitOfWork, ICloudStorageService cloudStorageService) : base(unitOfWork)
    {
        _cloudStorageService = cloudStorageService;
    }

    public async Task<Result<AttachmentViewModel>> Create(string contentType, Stream stream)
    {
        var attachment = UnitOfWork.Attachments.Add(new Attachment()
        {
            ContentType = contentType
        });

        await UnitOfWork.SaveChangesAsync();

        // Upload to Cloud
        var uploadResult = await _cloudStorageService.Upload(attachment.Id, contentType, stream);

        return uploadResult.IsSuccess
            ? new AttachmentViewModel()
            {
                Id = attachment.Id,
                ContentType = attachment.ContentType,
                Url = uploadResult.Value
            }
            : Error.Unexpected();
    }

    public async Task<Result> Delete(Guid id)
    {
        UnitOfWork.Attachments.Remove(new Attachment { Id = id });

        await UnitOfWork.SaveChangesAsync();

        var deleteResult = await _cloudStorageService.Delete(id);

        return deleteResult.IsSuccess ? Result.Success() : Error.Unexpected();
    }
}