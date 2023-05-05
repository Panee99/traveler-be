using Application.Configurations.Auth;
using Data.Enums;
using Microsoft.AspNetCore.Mvc;
using Service.Commons.Pagination;
using Service.Interfaces;
using Service.Models.Attachment;
using Service.Models.Tour;
using Shared.Helpers;

namespace Application.Controllers;

[Authorize(AccountRole.Manager)]
[Route("tours")]
public class ToursController : ApiController
{
    private readonly ITourService _tourService;
    private readonly ITravelerService _travelerService;
    private readonly ITourGroupService _tourGroupService;
    
    public ToursController(ITourService tourService, ITravelerService travelerService, ITourGroupService tourGroupService)
    {
        _tourService = tourService;
        _travelerService = travelerService;
        _tourGroupService = tourGroupService;
    }

    /// <summary>
    /// TOUR
    /// </summary>
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
    public async Task<IActionResult> GetDetails(Guid id)
    {
        var result = await _tourService.GetDetails(id);
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
    /// TRAVELER IN TOUR
    /// </summary>
    [Authorize]
    [HttpGet("{id:guid}/travelers")]
    public async Task<IActionResult> ListByTour(Guid id)
    {
        var result = await _travelerService.ListByTour(id);
        return result.Match(Ok, OnError);
    }

    /// <summary>
    /// THUMBNAIL
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

    /// <summary>
    /// CAROUSEL
    /// </summary>
    [ProducesResponseType(typeof(AttachmentViewModel), StatusCodes.Status200OK)]
    [HttpPost("{id:guid}/carousel")]
    public async Task<IActionResult> AddToCarousel(Guid id, IFormFile file)
    {
        var validateResult = FileHelper.ValidateImageFile(file);
        if (!validateResult.IsSuccess) return OnError(validateResult.Error);

        var result = await _tourService.AddToCarousel(id, file.ContentType, file.OpenReadStream());
        return result.Match(Ok, OnError);
    }

    [ProducesResponseType(StatusCodes.Status200OK)]
    [HttpDelete("{tourId:guid}/carousel/{attachmentId}")]
    public async Task<IActionResult> DeleteFromCarousel(Guid tourId, Guid attachmentId)
    {
        var result = await _tourService.DeleteFromCarousel(tourId, attachmentId);
        return result.Match(Ok, OnError);
    }

    [ProducesResponseType(typeof(List<AttachmentViewModel>), StatusCodes.Status200OK)]
    [HttpGet("{id:guid}/carousel")]
    [AllowAnonymous]
    public async Task<IActionResult> GetCarousel(Guid id)
    {
        var result = await _tourService.GetCarousel(id);
        return result.Match(Ok, OnError);
    }
    
    [HttpGet("{id:guid}/tour-groups")]
    public async Task<IActionResult> ListTourGroups(Guid id)
    {
        var result = await _tourGroupService.ListGroupsByTour(id);
        return result.Match(Ok, OnError);
    }
}