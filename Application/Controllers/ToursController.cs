using Application.Configurations.Auth;
using Data.Enums;
using Microsoft.AspNetCore.Mvc;
using Service.Interfaces;
using Service.Models.Attachment;
using Service.Models.Tour;
using Service.Pagination;
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
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Find(Guid id)
    {
        var result = await _tourService.Find(id);
        return result.Match(Ok, OnError);
    }

    [ProducesResponseType(typeof(AttachmentViewModel), StatusCodes.Status200OK)]
    [HttpPut("{id:guid}/thumbnail")]
    public async Task<IActionResult> UpdateThumbnail(Guid id, IFormFile file)
    {
        var validateResult = FileHelper.ValidateImageFile(file);
        if (!validateResult.IsSuccess) return OnError(validateResult.Error);

        var result = await _tourService.UpdateThumbnail(id, file.ContentType, file.OpenReadStream());
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
}