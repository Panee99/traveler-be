using Application.Configurations.Auth;
using Microsoft.AspNetCore.Mvc;
using Service.Interfaces;
using Service.Models.TourVariant;

namespace Application.Controllers;

[Authorize]
[Route("tour-variants")]
public class TourVariantsController : ApiController
{
    private readonly ITravelerService _travelerService;
    private readonly ITourVariantService _tourVariantService;

    public TourVariantsController(ITravelerService travelerService, ITourVariantService tourVariantService)
    {
        _travelerService = travelerService;
        _tourVariantService = tourVariantService;
    }

    /// <summary>
    /// Create a tour variant
    /// </summary>
    [HttpPost("")]
    public async Task<IActionResult> Create(TourVariantCreateModel model)
    {
        var result = await _tourVariantService.Create(model);
        return result.Match(Ok, OnError);
    }

    /// <summary>
    /// Update a tour variant
    /// </summary>
    [HttpPatch("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, TourVariantUpdateModel model)
    {
        var result = await _tourVariantService.Update(id, model);
        return result.Match(Ok, OnError);
    }

    /// <summary>
    /// Delete a tour variant
    /// </summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _tourVariantService.Delete(id);
        return result.Match(Ok, OnError);
    }

    /// <summary>
    /// Get a tour variant
    /// </summary>
    [AllowAnonymous]
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id)
    {
        var result = await _tourVariantService.Get(id);
        return result.Match(Ok, OnError);
    }
    
    /// <summary>
    /// List travelers in tour variant
    /// </summary>
    [HttpGet("{id:guid}/travelers")]
    public async Task<IActionResult> ListByTourVariant(Guid id)
    {
        var result = await _travelerService.ListByTourVariant(id);
        return result.Match(Ok, OnError);
    }
    
    /// <summary>
    /// List groups of this tour variant
    /// </summary>
    [HttpGet("{id:guid}/tour-groups")]
    public async Task<IActionResult> ListGroupsByTourVariant(Guid id)
    {
        var result = await _tourVariantService.ListGroupsByTourVariant(id);
        return result.Match(Ok, OnError);
    }
}