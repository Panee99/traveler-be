using System.Net;
using Application.Configurations.Auth;
using Microsoft.AspNetCore.Mvc;
using Service.Interfaces;
using Service.Models.Attachment;
using Service.Models.Location;
using Service.Pagination;
using Shared;
using Shared.Enums;
using Swashbuckle.AspNetCore.Annotations;

namespace Application.Controllers;

[Route("locations")]
public class LocationsController : ApiController
{
    private readonly ILocationService _locationService;

    public LocationsController(ILocationService locationService)
    {
        _locationService = locationService;
    }

    [ProducesResponseType(typeof(LocationViewModel), StatusCodes.Status201Created)]
    [Authorize(UserRole.Manager)]
    [HttpPost("")]
    public async Task<IActionResult> Create(LocationCreateModel model)
    {
        var result = await _locationService.Create(model);
        return result.Match(
            value => new ObjectResult(value) { StatusCode = (int)HttpStatusCode.Created },
            OnError);
    }

    [ProducesResponseType(typeof(LocationViewModel), StatusCodes.Status200OK)]
    [Authorize(UserRole.Manager)]
    [HttpPatch("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, LocationUpdateModel model)
    {
        var result = await _locationService.Update(id, model);
        return result.Match(Ok, OnError);
    }

    [ProducesResponseType(StatusCodes.Status200OK)]
    [Authorize(UserRole.Manager)]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _locationService.Delete(id);
        return result.Match(Ok, OnError);
    }

    [ProducesResponseType(typeof(LocationViewModel), StatusCodes.Status200OK)]
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Find(Guid id)
    {
        var result = await _locationService.Find(id);
        return result.Match(Ok, OnError);
    }

    [SwaggerOperation(description: "File size < 5MB")]
    [Authorize(UserRole.Manager)]
    [ProducesResponseType(typeof(AttachmentViewModel), StatusCodes.Status200OK)]
    [HttpPost("{locationId:guid}/attachments")]
    public async Task<IActionResult> AddAttachment([FromRoute] Guid locationId, IFormFile file)
    {
        if (file.Length > AppConstants.FileSizeMax) return BadRequest("File too big, max = 5mb");

        var result = await _locationService.CreateAttachment(locationId,
            new AttachmentCreateModel(file.ContentType, file.OpenReadStream()));

        return result.Match(Ok, OnError);
    }

    [Authorize(UserRole.Manager)]
    [HttpDelete("{locationId:guid}/attachments/{attachmentId:guid}")]
    public async Task<IActionResult> DeleteAttachment(Guid locationId, Guid attachmentId)
    {
        var result = await _locationService.DeleteAttachment(locationId, attachmentId);
        return result.Match(Ok, OnError);
    }

    [ProducesResponseType(typeof(List<AttachmentViewModel>), StatusCodes.Status200OK)]
    [HttpGet("{id:guid}/attachments")]
    public async Task<IActionResult> ListAttachments(Guid id)
    {
        var result = await _locationService.ListAttachments(id);
        return result.Match(Ok, OnError);
    }

    [ProducesResponseType(typeof(PaginationModel<LocationViewModel>), StatusCodes.Status200OK)]
    [HttpPost("filter")]
    public async Task<IActionResult> Filter(LocationFilterModel model)
    {
        var result = await _locationService.Filter(model);
        return result.Match(Ok, OnError);
    }
}