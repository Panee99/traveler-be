using Application.Configurations.Auth;
using Data.Enums;
using Microsoft.AspNetCore.Mvc;
using Service.Commons.Pagination;
using Service.Interfaces;
using Service.Models.Tour;

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
    [ProducesResponseType(typeof(TourViewModel), StatusCodes.Status200OK)]
    [HttpPost("")]
    public async Task<IActionResult> Create(TourCreateModel model)
    {
        var result = await _tourService.Create(model);
        return result.Match(Ok, OnError);
    }

    /// <summary>
    /// Update a tour
    /// </summary>
    [ProducesResponseType(typeof(TourViewModel), StatusCodes.Status200OK)]
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
    [ProducesResponseType(typeof(TourViewModel), StatusCodes.Status200OK)]
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
    [ProducesResponseType(typeof(PaginationModel<TourFilterViewModel>), StatusCodes.Status200OK)]
    [AllowAnonymous]
    [HttpPost("filter")]
    public async Task<IActionResult> Filter(TourFilterModel model)
    {
        var result = await _tourService.Filter(model);
        return result.Match(Ok, OnError);
    }

    
}