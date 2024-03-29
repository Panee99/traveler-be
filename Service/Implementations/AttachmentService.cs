﻿using Data.EFCore;
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

    public async Task<Result<AttachmentViewModel>> Create(string extension, string contentType, Stream stream)
    {
        var attachment = UnitOfWork.Attachments.Add(new Attachment()
        {
            Extension = extension,
            ContentType = contentType
        });

        await UnitOfWork.SaveChangesAsync();

        // Upload to Cloud
        var uploadResult = await _cloudStorageService.Upload(attachment.FileName, contentType, stream);

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
        var attachment = await UnitOfWork.Attachments.FindAsync(id);
        if (attachment is null) return Error.NotFound();

        UnitOfWork.Attachments.Remove(attachment);
        await UnitOfWork.SaveChangesAsync();

        var deleteResult = await _cloudStorageService.Delete(attachment.FileName);
        return deleteResult.IsSuccess ? Result.Success() : Error.Unexpected();
    }
}