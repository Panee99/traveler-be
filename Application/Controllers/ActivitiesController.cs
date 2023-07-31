using Application.Configurations.Auth;
using Microsoft.AspNetCore.Mvc;
using Service.Interfaces;
using Service.Models.Activity;

namespace Application.Controllers;

[Authorize]
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
    [ProducesResponseType(StatusCodes.Status200OK)]
    [HttpPost("")]
    public async Task<IActionResult> Create(PartialActivityModel model)
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
    /// Delete an draft activity
    /// </summary>
    [ProducesResponseType(StatusCodes.Status200OK)]
    [HttpDelete("draft/{id:guid}")]
    public async Task<IActionResult> DeleteDraft(Guid id)
    {
        var result = await _activityService.DeleteDraft(id);
        return result.Match(Ok, OnError);
    }

    /// <summary>
    /// Update an activity
    /// </summary>
    [ProducesResponseType(StatusCodes.Status200OK)]
    [HttpPatch]
    public async Task<IActionResult> Update(PartialActivityModel model)
    {
        var result = await _activityService.Update(model);
        return result.Match(Ok, OnError);
    }

    /// <summary>
    /// Attend
    /// </summary>
    [ProducesResponseType(StatusCodes.Status200OK)]
    [HttpPost("attend/{code:guid}")]
    public async Task<IActionResult> Attend(Guid code)
    {
        var result = await _activityService.Attend(code);
        return result.Match(Ok, OnError);
    }
}