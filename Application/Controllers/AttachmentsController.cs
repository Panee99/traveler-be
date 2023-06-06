using Application.Configurations.Auth;
using Microsoft.AspNetCore.Mvc;
using Service.Interfaces;
using Service.Models.Attachment;

namespace Application.Controllers;

[Authorize]
[Route("attachments")]
public class AttachmentsController : ApiController
{
    private readonly IAttachmentService _attachmentService;

    public AttachmentsController(IAttachmentService attachmentService)
    {
        _attachmentService = attachmentService;
    }

    /// <summary>
    /// Create attachment and return it's Id
    /// </summary>
    [ProducesResponseType(typeof(AttachmentViewModel), StatusCodes.Status200OK)]
    [HttpPost]
    public async Task<IActionResult> Upload(IFormFile file)
    {
        var result = await _attachmentService.Create(file.ContentType, file.OpenReadStream());
        return result.Match(Ok, OnError);
    }
}