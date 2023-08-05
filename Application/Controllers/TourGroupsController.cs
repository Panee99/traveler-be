using Application.Configurations.Auth;
using Data.Enums;
using Microsoft.AspNetCore.Mvc;
using Service.Interfaces;
using Service.Models.Activity;
using Service.Models.TourGroup;
using Service.Models.User;

namespace Application.Controllers;

[Route("tour-groups")]
public class TourGroupsController : ApiController
{
    private readonly ITourGroupService _tourGroupService;

    public TourGroupsController(ITourGroupService tourGroupService)
    {
        _tourGroupService = tourGroupService;
    }

    /// <summary>
    /// Start a tour group
    /// </summary>
    [ProducesResponseType(StatusCodes.Status200OK)]
    [HttpPut("{id:guid}/start")]
    public async Task<IActionResult> Start(Guid id)
    {
        var result = await _tourGroupService.Start(id);
        return result.Match(Ok, OnError);
    }

    /// <summary>
    /// End a tour group
    /// </summary>
    [ProducesResponseType(StatusCodes.Status200OK)]
    [HttpPut("{id:guid}/end")]
    public async Task<IActionResult> End(Guid id)
    {
        var result = await _tourGroupService.End(id);
        return result.Match(Ok, OnError);
    }

    /// <summary>
    /// Cancel a tour group
    /// </summary>
    [ProducesResponseType(StatusCodes.Status200OK)]
    [HttpPut("{id:guid}/cancel")]
    public async Task<IActionResult> Cancel(Guid id)
    {
        var result = await _tourGroupService.Cancel(id);
        return result.Match(Ok, OnError);
    }

    /// <summary>
    /// Get a tour group
    /// </summary>
    [ProducesResponseType(typeof(List<TourGroupViewModel>), StatusCodes.Status200OK)]
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id)
    {
        var result = await _tourGroupService.Get(id);
        return result.Match(Ok, OnError);
    }

    /// <summary>
    /// List all travelers and tour guide of a group
    /// </summary>
    [ProducesResponseType(typeof(List<UserViewModel>), StatusCodes.Status200OK)]
    [HttpGet("{id:guid}/members")]
    public async Task<IActionResult> ListMembers([FromRoute] Guid id)
    {
        var result = await _tourGroupService.ListMembers(id);
        return result.Match(Ok, OnError);
    }

    /// <summary>
    /// List all activities of a tour group
    /// </summary>
    [ProducesResponseType(typeof(List<ActivityViewModel>), StatusCodes.Status200OK)]
    [HttpGet("{id:guid}/activities")]
    public async Task<IActionResult> ListActivities(Guid id)
    {
        var result = await _tourGroupService.ListActivities(id);
        return result.Match(Ok, OnError);
    }

    [Authorize(UserRole.Traveler, UserRole.TourGuide)]
    [HttpPost("{id:guid}/emergency")]
    public async Task<IActionResult> SendEmergency(Guid id, EmergencyRequestModel model)
    {
        var result = await _tourGroupService.SendEmergency(id, CurrentUser.Id, model);
        return result.Match(Ok, OnError);
    }
}