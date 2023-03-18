using System.Net;
using Application.Configurations.Auth;
using Microsoft.AspNetCore.Mvc;
using Service.Interfaces;
using Service.Models.Attachment;
using Service.Models.Location;
using Shared.Enums;

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

    // 5_MB
    private const int FileSizeMax = 5 * 1024 * 1024;
    [HttpPost("{id:guid}/attachments")]
    public async Task<IActionResult> AddAttachments(Guid id, [FromForm] IFormFile file)
    {
        if (file.Length > FileSizeMax) return BadRequest("File too big, max = 5mb");

        Console.WriteLine(file.Length);
        Console.WriteLine(file.ContentType);

        var result = await _locationService.CreateAttachment(id,
            new AttachmentCreateModel(file.ContentType, file.OpenReadStream()));

        return result.Match(Ok, OnError);
    }
}