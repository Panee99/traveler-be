using Microsoft.AspNetCore.Mvc;
using Service.Interfaces;
using Service.Models.Activity;

namespace Application.Controllers;

[Route("activities")]
public class ActivitiesController : ApiController
{
    private readonly IActivityService _activityService;

    public ActivitiesController(IActivityService activityService)
    {
        _activityService = activityService;
    }

    /// <summary>
    /// Create an activity
    /// </summary>
    [ProducesResponseType(typeof(ActivityViewModel), StatusCodes.Status200OK)]
    [HttpPost("")]
    public async Task<IActionResult> Create(CreateActivityModel model)
    {
        var result = await _activityService.Create(model);
        return result.Match(Ok, OnError);
    }

    /// <summary>
    /// Delete an activity
    /// </summary>
    [ProducesResponseType(StatusCodes.Status200OK)]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _activityService.Delete(id);
        return result.Match(Ok, OnError);
    }

    /// <summary>
    /// Update an activity
    /// </summary>
    [ProducesResponseType(typeof(ActivityViewModel), StatusCodes.Status200OK)]
    [HttpPatch("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, ActivityUpdateModel model)
    {
        // var result = await _activityService.Update(id, model);
        // return result.Match(Ok, OnError);
        throw new NotImplementedException();
    }
}