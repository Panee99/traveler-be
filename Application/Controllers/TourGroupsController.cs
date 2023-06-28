using Application.Configurations.Auth;
using Data.Enums;
using Microsoft.AspNetCore.Mvc;
using Service.Interfaces;
using Service.Models.TourGroup;

namespace Application.Controllers;

[Route("tour-groups")]
public class TourGroupsController : ApiController
{
    private readonly ITourGroupService _tourGroupService;
    private readonly IIncurredCostService _incurredCostService;

    public TourGroupsController(ITourGroupService tourGroupService,
        IIncurredCostService incurredCostService)
    {
        _tourGroupService = tourGroupService;
        _incurredCostService = incurredCostService;
    }

    /// <summary>
    /// Get a tour group
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id)
    {
        var result = await _tourGroupService.Get(id);
        return result.Match(Ok, OnError);
    }

    /// <summary>
    /// Create a new tour group
    /// </summary>
    [Authorize(UserRole.Admin)]
    [HttpPost("")]
    public async Task<IActionResult> Create(TourGroupCreateModel model)
    {
        var result = await _tourGroupService.Create(model);
        return result.Match(Ok, OnError);
    }

    /// <summary>
    /// Update a tour group
    /// </summary>
    [Authorize(UserRole.Admin)]
    [HttpPatch("{id:guid}")]
    public async Task<IActionResult> Update([FromRoute] Guid id, TourGroupUpdateModel model)
    {
        var result = await _tourGroupService.Update(id, model);
        return result.Match(Ok, OnError);
    }

    /// <summary>
    /// Delete a tour group
    /// </summary>
    [Authorize(UserRole.Admin)]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete([FromRoute] Guid id)
    {
        var result = await _tourGroupService.Delete(id);
        return result.Match(Ok, OnError);
    }

    /// <summary>
    /// Add travelers to group
    /// </summary>
    [HttpPatch("{id:guid}/travelers")]
    public async Task<IActionResult> AddTravelers([FromRoute] Guid id, [FromBody] List<Guid> travelerIds)
    {
        var result = await _tourGroupService.AddTravelers(id, travelerIds);
        return result.Match(Ok, OnError);
    }

    /// <summary>
    /// Remove travelers from group
    /// </summary>
    [HttpDelete("{id:guid}/travelers")]
    public async Task<IActionResult> RemoveTravelers([FromRoute] Guid id, [FromBody] List<Guid> travelerIds)
    {
        var result = await _tourGroupService.RemoveTravelers(id, travelerIds);
        return result.Match(Ok, OnError);
    }

    /// <summary>
    /// List all travelers of a group
    /// </summary>
    [HttpGet("{id:guid}/members")]
    public async Task<IActionResult> ListMembers([FromRoute] Guid id)
    {
        var result = await _tourGroupService.ListMembers(id);
        return result.Match(Ok, OnError);
    }

    /// <summary>
    /// List all activities of a tour group
    /// </summary>
    [HttpGet("{id:guid}/activities")]
    public async Task<IActionResult> ListActivities(Guid id)
    {
        var result = await _tourGroupService.ListActivities(id);
        return result.Match(Ok, OnError);
    }

    /// <summary>
    /// List all Incurred Costs in group
    /// </summary>
    [HttpGet("{id:guid}/incurred-costs")]
    public async Task<IActionResult> ListIncurredCosts(Guid id)
    {
        var result = await _incurredCostService.ListAll(id);
        return result.Match(Ok, OnError);
    }
}