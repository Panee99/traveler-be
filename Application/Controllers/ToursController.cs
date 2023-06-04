using Application.Configurations.Auth;
using Data.Enums;
using Microsoft.AspNetCore.Mvc;
using Service.Commons.QueryExtensions;
using Service.Interfaces;
using Service.Models.Tour;
using Service.Models.TourVariant;

namespace Application.Controllers;

[Authorize(UserRole.Admin)]
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

    /// <summary>
    /// Create a new tour
    /// </summary>
    [ProducesResponseType(typeof(TourDetailsViewModel), StatusCodes.Status200OK)]
    [HttpPost("")]
    public async Task<IActionResult> Create(TourCreateModel model)
    {
        var result = await _tourService.Create(model);
        return result.Match(Ok, OnError);
    }

    /// <summary>
    /// Update a tour
    /// </summary>
    [ProducesResponseType(typeof(TourDetailsViewModel), StatusCodes.Status200OK)]
    [HttpPatch("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, TourUpdateModel model)
    {
        var result = await _tourService.Update(id, model);
        return result.Match(Ok, OnError);
    }

    /// <summary>
    /// Delete a tour
    /// </summary>
    [ProducesResponseType(StatusCodes.Status200OK)]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _tourService.Delete(id);
        return result.Match(Ok, OnError);
    }

    /// <summary>
    /// Get details of a tour
    /// </summary>
    [ProducesResponseType(typeof(TourDetailsViewModel), StatusCodes.Status200OK)]
    [AllowAnonymous]
    [HttpGet("{id:guid}/details")]
    public async Task<IActionResult> GetDetails(Guid id)
    {
        var result = await _tourService.GetDetails(id);
        return result.Match(Ok, OnError);
    }

    /// <summary>
    /// Filter tours
    /// </summary>
    [ProducesResponseType(typeof(PaginationModel<TourViewModel>), StatusCodes.Status200OK)]
    [AllowAnonymous]
    [HttpPost("filter")]
    public async Task<IActionResult> Filter(TourFilterModel model)
    {
        var result = await _tourService.Filter(model);
        return result.Match(Ok, OnError);
    }

    /// <summary>
    /// List all variants of a tour
    /// </summary>
    [ProducesResponseType(typeof(List<TourVariantViewModel>), StatusCodes.Status200OK)]
    [AllowAnonymous]
    [HttpPost("{id:guid}/tour-variants")]
    public async Task<IActionResult> ListTourVariants(Guid id)
    {
        var result = await _tourService.ListTourVariants(id);
        return result.Match(Ok, OnError);
    }
    
    /// <summary>
    /// Get tour flow of a tour
    /// </summary>
    [ProducesResponseType(typeof(List<TourVariantViewModel>), StatusCodes.Status200OK)]
    [AllowAnonymous]
    [HttpPost("{id:guid}/tour-flow")]
    public async Task<IActionResult> GetTourFlow(Guid id)
    {
        var result = await _tourService.GetTourFlow(id);
        return result.Match(Ok, OnError);
    }
}