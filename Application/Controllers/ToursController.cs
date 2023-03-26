using Application.Configurations.Auth;
using Microsoft.AspNetCore.Mvc;
using Service.Interfaces;
using Service.Models.Attachment;
using Service.Models.Tour;
using Service.Pagination;
using Shared.Enums;
using Shared.Helpers;

namespace Application.Controllers;

[Route("tours")]
public class ToursController : ApiController
{
    private readonly ITourService _tourService;
    private readonly ITourGroupService _tourGroupService;

    public ToursController(ITourService tourService, ITourGroupService tourGroupService)
    {
        _tourService = tourService;
        _tourGroupService = tourGroupService;
    }

    [ProducesResponseType(typeof(TourViewModel), StatusCodes.Status200OK)]
    [Authorize(UserRole.Manager)]
    [HttpPost("")]
    public async Task<IActionResult> Create(TourCreateModel model)
    {
        var result = await _tourService.Create(model);
        return result.Match(Ok, OnError);
    }

    [ProducesResponseType(typeof(TourViewModel), StatusCodes.Status200OK)]
    [Authorize(UserRole.Manager)]
    [HttpPatch("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, TourUpdateModel model)
    {
        var result = await _tourService.Update(id, model);
        return result.Match(Ok, OnError);
    }

    [ProducesResponseType(StatusCodes.Status200OK)]
    [Authorize(UserRole.Manager)]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _tourService.Delete(id);
        return result.Match(Ok, OnError);
    }

    [ProducesResponseType(typeof(TourViewModel), StatusCodes.Status200OK)]
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Find(Guid id)
    {
        var result = await _tourService.Find(id);
        return result.Match(Ok, OnError);
    }

    [ProducesResponseType(typeof(AttachmentViewModel), StatusCodes.Status200OK)]
    [Authorize(UserRole.Manager)]
    [HttpPut("{id:guid}/thumbnail")]
    public async Task<IActionResult> UpdateThumbnail(Guid id, IFormFile file)
    {
        var validateResult = FileHelper.ValidateImageFile(file);
        if (!validateResult.IsSuccess) return OnError(validateResult.Error);

        var result = await _tourService.UpdateThumbnail(id, file.ContentType, file.OpenReadStream());
        return result.Match(Ok, OnError);
    }

    [ProducesResponseType(typeof(PaginationModel<TourFilterViewModel>), StatusCodes.Status200OK)]
    [HttpGet("filter")]
    public async Task<IActionResult> Filter(TourFilterModel model)
    {
        var result = await _tourService.Filter(model);
        return result.Match(Ok, OnError);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> ListTourGroups([FromRoute] Guid id)
    {
        var result = await _tourGroupService.ListGroupsByTour(id);
        return result.Match(Ok, OnError);
    }
}