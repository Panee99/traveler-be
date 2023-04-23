using Application.Configurations.Auth;
using Data.Enums;
using Microsoft.AspNetCore.Mvc;
using Service.Commons.Pagination;
using Service.Interfaces;
using Service.Models.Attachment;
using Service.Models.Location;
using Service.Models.Tour;
using Shared.Helpers;

namespace Application.Controllers;

[Authorize(AccountRole.Manager)]
[Route("tours")]
public class ToursController : ApiController
{
    private readonly ITourService _tourService;

    public ToursController(ITourService tourService)
    {
        _tourService = tourService;
    }

    [ProducesResponseType(typeof(TourViewModel), StatusCodes.Status200OK)]
    [HttpPost("")]
    public async Task<IActionResult> Create(TourCreateModel model)
    {
        var result = await _tourService.Create(model);
        return result.Match(Ok, OnError);
    }

    [ProducesResponseType(typeof(TourViewModel), StatusCodes.Status200OK)]
    [HttpPatch("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, TourUpdateModel model)
    {
        var result = await _tourService.Update(id, model);
        return result.Match(Ok, OnError);
    }

    [ProducesResponseType(StatusCodes.Status200OK)]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _tourService.Delete(id);
        return result.Match(Ok, OnError);
    }

    [ProducesResponseType(typeof(TourViewModel), StatusCodes.Status200OK)]
    [AllowAnonymous]
    [HttpGet("{id:guid}/details")]
    public async Task<IActionResult> Find(Guid id)
    {
        var result = await _tourService.Find(id);
        return result.Match(Ok, OnError);
    }

    [ProducesResponseType(typeof(PaginationModel<TourFilterViewModel>), StatusCodes.Status200OK)]
    [AllowAnonymous]
    [HttpPost("filter")]
    public async Task<IActionResult> Filter(TourFilterModel model)
    {
        var result = await _tourService.Filter(model);
        return result.Match(Ok, OnError);
    }

    /// <summary>
    /// ATTACHMENTS
    /// </summary>
    [ProducesResponseType(typeof(AttachmentViewModel), StatusCodes.Status200OK)]
    [HttpPut("{id:guid}/thumbnail")]
    public async Task<IActionResult> UpdateThumbnail(Guid id, IFormFile file)
    {
        var validateResult = FileHelper.ValidateImageFile(file);
        if (!validateResult.IsSuccess) return OnError(validateResult.Error);

        var result = await _tourService.UpdateThumbnail(id, file.ContentType, file.OpenReadStream());
        return result.Match(Ok, OnError);
    }

    [ProducesResponseType(typeof(AttachmentViewModel), StatusCodes.Status200OK)]
    [HttpPost("{id:guid}/attachments")]
    public async Task<IActionResult> AddAttachment(Guid id, IFormFile file)
    {
        var validateResult = FileHelper.ValidateImageFile(file);
        if (!validateResult.IsSuccess) return OnError(validateResult.Error);

        var result = await _tourService.AddAttachment(id, file.ContentType, file.OpenReadStream());
        return result.Match(Ok, OnError);
    }

    [ProducesResponseType(StatusCodes.Status200OK)]
    [HttpDelete("{tourId:guid}/attachments/{attachmentId}")]
    public async Task<IActionResult> DeleteAttachment(Guid tourId, Guid attachmentId)
    {
        var result = await _tourService.DeleteAttachment(tourId, attachmentId);
        return result.Match(Ok, OnError);
    }

    [ProducesResponseType(typeof(List<AttachmentViewModel>), StatusCodes.Status200OK)]
    [HttpGet("{tourId:guid}/attachments")]
    [AllowAnonymous]
    public async Task<IActionResult> ListAttachments(Guid tourId)
    {
        var result = await _tourService.ListAttachments(tourId);
        return result.Match(Ok, OnError);
    }

    /// <summary>
    /// LOCATIONS
    /// </summary>
    [ProducesResponseType(typeof(LocationViewModel), StatusCodes.Status200OK)]
    [HttpPost("{id:guid}/locations")]
    public async Task<IActionResult> AddLocation(Guid id, LocationCreateModel model)
    {
        var result = await _tourService.AddLocation(id, model);
        return result.Match(Ok, OnError);
    }

    [ProducesResponseType(typeof(LocationViewModel), StatusCodes.Status200OK)]
    [HttpPatch("/locations/{locationId:guid}")]
    public async Task<IActionResult> UpdateLocation(Guid locationId, LocationUpdateModel model)
    {
        var result = await _tourService.UpdateLocation(locationId, model);
        return result.Match(Ok, OnError);
    }

    [ProducesResponseType(typeof(LocationViewModel), StatusCodes.Status200OK)]
    [HttpDelete("/locations/{locationId:guid}")]
    public async Task<IActionResult> DeleteLocation(Guid locationId)
    {
        var result = await _tourService.DeleteLocation(locationId);
        return result.Match(Ok, OnError);
    }

    [ProducesResponseType(typeof(LocationViewModel), StatusCodes.Status200OK)]
    [HttpGet("{id:guid}/locations")]
    [AllowAnonymous]
    public async Task<IActionResult> ListLocations(Guid id)
    {
        var result = await _tourService.ListLocations(id);
        return result.Match(Ok, OnError);
    }
}