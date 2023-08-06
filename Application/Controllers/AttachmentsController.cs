using Application.Configurations.Auth;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Service.Interfaces;
using Service.Models.Attachment;
using Shared.ResultExtensions;

namespace Application.Controllers;

[Authorize]
[Route("attachments")]
public class AttachmentsController : ApiController
{
    private const long FileSizeMax = 104_857_600; // 100 MB
    private readonly IAttachmentService _attachmentService;

    public AttachmentsController(IAttachmentService attachmentService)
    {
        _attachmentService = attachmentService;
    }

    /// <summary>
    /// Create attachment and return it's id and link
    /// </summary>
    [ProducesResponseType(typeof(AttachmentViewModel), StatusCodes.Status200OK)]
    [RequestFormLimits(MultipartBodyLengthLimit = FileSizeMax, ValueCountLimit = 1)]
    [RequestSizeLimit(long.MaxValue)]
    [HttpPost]
    public async Task<IActionResult> Upload(IFormFile file)
    {
        var extension = Path.GetExtension(file.FileName);
        if (extension.IsNullOrEmpty()) return OnError(Error.Validation("Invalid extension"));
        var result = await _attachmentService.Create(extension, file.ContentType, file.OpenReadStream());
        return result.Match(Ok, OnError);
    }
}