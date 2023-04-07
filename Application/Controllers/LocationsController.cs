﻿using System.Net;
using Application.Configurations.Auth;
using Data.Enums;
using Microsoft.AspNetCore.Mvc;
using Service.Interfaces;
using Service.Models.Attachment;
using Service.Models.Location;
using Service.Pagination;
using Shared.Helpers;

namespace Application.Controllers;

[Authorize(AccountRole.Manager)]
[Route("locations")]
public class LocationsController : ApiController
{
    private readonly ILocationService _locationService;

    public LocationsController(ILocationService locationService)
    {
        _locationService = locationService;
    }

    [ProducesResponseType(typeof(LocationViewModel), StatusCodes.Status201Created)]
    [HttpPost("")]
    public async Task<IActionResult> Create(LocationCreateModel model)
    {
        var result = await _locationService.Create(model);
        return result.Match(
            value => new ObjectResult(value) { StatusCode = (int)HttpStatusCode.Created },
            OnError);
    }

    [ProducesResponseType(typeof(LocationViewModel), StatusCodes.Status200OK)]
    [HttpPatch("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, LocationUpdateModel model)
    {
        var result = await _locationService.Update(id, model);
        return result.Match(Ok, OnError);
    }

    [ProducesResponseType(StatusCodes.Status200OK)]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _locationService.Delete(id);
        return result.Match(Ok, OnError);
    }

    [ProducesResponseType(typeof(AttachmentViewModel), StatusCodes.Status200OK)]
    [HttpPost("{locationId:guid}/attachments")]
    public async Task<IActionResult> AddAttachment([FromRoute] Guid locationId, IFormFile file)
    {
        var validateResult = FileHelper.ValidateImageFile(file);
        if (!validateResult.IsSuccess) return OnError(validateResult.Error);

        var result = await _locationService.CreateAttachment(locationId, file.ContentType, file.OpenReadStream());

        return result.Match(Ok, OnError);
    }

    [ProducesResponseType(StatusCodes.Status200OK)]
    [HttpDelete("{locationId:guid}/attachments/{attachmentId:guid}")]
    public async Task<IActionResult> DeleteAttachment(Guid locationId, Guid attachmentId)
    {
        var result = await _locationService.DeleteAttachment(locationId, attachmentId);
        return result.Match(Ok, OnError);
    }

    [ProducesResponseType(typeof(List<AttachmentViewModel>), StatusCodes.Status200OK)]
    [AllowAnonymous]
    [HttpGet("{id:guid}/attachments")]
    public async Task<IActionResult> ListAttachments(Guid id)
    {
        var result = await _locationService.ListAttachments(id);
        return result.Match(Ok, OnError);
    }

    [ProducesResponseType(typeof(LocationViewModel), StatusCodes.Status200OK)]
    [AllowAnonymous]
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Find(Guid id)
    {
        var result = await _locationService.Find(id);
        return result.Match(Ok, OnError);
    }

    [ProducesResponseType(typeof(PaginationModel<LocationViewModel>), StatusCodes.Status200OK)]
    [AllowAnonymous]
    [HttpPost("filter")]
    public async Task<IActionResult> Filter(LocationFilterModel model)
    {
        var result = await _locationService.Filter(model);
        return result.Match(Ok, OnError);
    }
}